using Application.DTOs.Seat;
using Application.Models;
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Application.Services
{
    // Service implementation for seat management.
    public class SeatService : ISeatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SeatService> _logger;
        private readonly IUserRepository _userRepository;  
        private readonly IPricingService _pricingService;

        public SeatService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SeatService> logger, IUserRepository userRepository, IPricingService pricingService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _userRepository = userRepository;
            _pricingService = pricingService;
        }
 
        public async Task<ServiceResult<List<SeatDto>>> ReserveSeatsAsync(int bookingId, List<ReserveSeatDto> reservesDto)
        {
            var reservedSeatDtos = new List<SeatDto>();
            foreach (var reserve in reservesDto)
            {
                var seat = await _unitOfWork.Seats.GetWithCabinClassAsync(reserve.SeatId);
                if (seat == null)
                {
                    return ServiceResult<List<SeatDto>>.Failure($"Seat with ID {reserve.SeatId} not found.");
                }

                // BookingId and PassengerId must be passed as separate coefficients in an array.
                var bookingPassenger = await _unitOfWork.BookingPassengers.GetByIdAsync(
                    new object[] { bookingId, reserve.PassengerId }
                    );
                

                if (bookingPassenger == null)
                {
                    return ServiceResult<List<SeatDto>>.Failure($"Passenger with ID {reserve.PassengerId} not found in booking {bookingId}.");
                }

                // Assign the seat
                bookingPassenger.SeatAssignmentId = seat.SeatId;
                _unitOfWork.BookingPassengers.Update(bookingPassenger);

                reservedSeatDtos.Add(new SeatDto
                {
                    SeatId = seat.SeatId,
                    SeatNumber = seat.SeatNumber,
                    CabinClassName = seat.CabinClass.Name
                });
            }

            await _unitOfWork.SaveChangesAsync();
            return ServiceResult<List<SeatDto>>.Success(reservedSeatDtos);
        }

        // Retrieves the seat map for a specific flight instance.
        public async Task<ServiceResult<SeatMapDto>> GetSeatMapForFlightAsync(int flightInstanceId)
        {
            _logger.LogInformation("Generating seat map for FlightInstanceId {FlightId}.", flightInstanceId);
            try
            {
                // 1. Get Flight Instance to find the assigned Aircraft
                var flightInstance = await _unitOfWork.FlightInstances.GetWithDetailsAsync(flightInstanceId);
                if (flightInstance?.Aircraft == null)
                {
                    return ServiceResult<SeatMapDto>.Failure("Flight instance or assigned aircraft not found.");
                }
                var aircraftTailNumber = flightInstance.AircraftId;

                // 2. Get all active seats for this aircraft
                var allSeatsOnAircraft = await _unitOfWork.Seats.GetSeatsByAircraftAsync(aircraftTailNumber);
                if (!allSeatsOnAircraft.Any())
                {
                    return ServiceResult<SeatMapDto>.Failure($"No seats defined for aircraft {aircraftTailNumber}.");
                }

                // 3. Get reserved seat IDs for *this specific flight instance*
                var reservedSeatIds = (await _unitOfWork.BookingPassengers.GetAssignmentsByFlightAsync(flightInstanceId)) // Needs new repo method
                                     .Where(bp => bp.SeatAssignmentId != null)
                                     .Select(bp => bp.SeatAssignmentId)
                                     .ToHashSet();

                // 4. Map Seats to DTOs and mark availability
                var seatDtos = _mapper.Map<List<SeatDto>>(allSeatsOnAircraft);
                foreach (var seatDto in seatDtos)
                {
                    seatDto.IsAvailable = !reservedSeatIds.Contains(seatDto.SeatId);
                    // TODO: Calculate Seat Price based on features/location/fare class (requires more logic)

                    // 5. Calculate Price  
                    // Call the pricing service to calculate the price of this specific seat
                    // (We will create this function in the next step)
                    var priceResult = await _pricingService.CalculateSeatPriceAsync(seatDto.SeatId, flightInstanceId);

                    if (priceResult.IsSuccess && priceResult.Data > 0)
                    {
                        seatDto.SeatPrice = priceResult.Data;
                    }
                    else
                    {
                        seatDto.SeatPrice = null; // Free or error
                    }

                }

                // 5. Group by Cabin Class
                var cabinLayouts = seatDtos
                    .GroupBy(s => s.CabinClassId)
                    .Select(g => new CabinSeatLayoutDto
                    {
                        CabinClassId = g.Key,
                        CabinClassName = g.First().CabinClassName, // Assumes all seats in group have same name
                        Seats = g.OrderBy(s => s.SeatNumber).ToList() // Order seats within cabin
                    })
                    .OrderBy(cl => cl.CabinClassId) // Order cabins (e.g., F, J, Y)
                    .ToList();

                // 6. Assemble the final SeatMapDto
                var seatMapDto = new SeatMapDto
                {
                    FlightInstanceId = flightInstanceId,
                    AircraftModel = flightInstance.Aircraft.AircraftType?.Model ?? "N/A",
                    AircraftTailNumber = aircraftTailNumber,
                    CabinLayouts = cabinLayouts
                };

                return ServiceResult<SeatMapDto>.Success(seatMapDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating seat map for FlightInstanceId {FlightId}.", flightInstanceId);
                return ServiceResult<SeatMapDto>.Failure("An error occurred while generating the seat map.");
            }
        }

       

        // Retrieves only the available seats for a flight instance, optionally filtered by cabin class.
        public async Task<ServiceResult<IEnumerable<SeatDto>>> GetAvailableSeatsAsync(SeatAvailabilityRequestDto request)
        {
            _logger.LogInformation("Getting available seats for FlightInstanceId {FlightId}, CabinClassId {CabinId}.",
                request.FlightInstanceId, request.CabinClassId?.ToString() ?? "All");
            try
            {
                // 1. Get Flight Instance to find Aircraft ID
                var flightInstance = await _unitOfWork.FlightInstances.GetActiveByIdAsync(request.FlightInstanceId);
                if (flightInstance == null) return ServiceResult<IEnumerable<SeatDto>>.Failure("Flight instance not found.");
 
                var aircraftId = flightInstance.AircraftId; // Extract the AircraftId here

                // 2. Get reserved seat IDs for this flight (Ensure SeatAssignmentId is not null before selecting)
                var reservedSeatIds = (await _unitOfWork.BookingPassengers.GetAssignmentsByFlightAsync(request.FlightInstanceId))
                                     .Where(bp => bp.SeatAssignmentId != null) // Ensure SeatAssignmentId is not null
                                     .Select(bp => bp.SeatAssignmentId!) // Use null-forgiving operator or handle potential null
                                     .ToHashSet();

                // 3. Get all seats on the aircraft, filter by cabin if specified
                var availableSeats = await _unitOfWork.Seats.GetAvailableSeatsForFlightAsync(
                    request.FlightInstanceId, // Pass FlightInstanceId if needed by repo logic, otherwise maybe not needed
                    aircraftId,             // Use the declared aircraftId variable
                    reservedSeatIds,
                    request.CabinClassId);

                var dtos = _mapper.Map<IEnumerable<SeatDto>>(availableSeats);
                // Mark as available and calculate price (IsAvailable is somewhat redundant now)
                foreach (var dto in dtos)
                {
                    dto.IsAvailable = true;
                    // TODO: Calculate price if needed
                }

                return ServiceResult<IEnumerable<SeatDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available seats for FlightInstanceId {FlightId}.", request.FlightInstanceId);
                return ServiceResult<IEnumerable<SeatDto>>.Failure("An error occurred while retrieving available seats.");
            }
        }

        // Assigns a specific seat to a passenger within a booking.
        public async Task<ServiceResult> AssignSeatAsync(AssignSeatRequestDto request, ClaimsPrincipal user)
        {
            _logger.LogInformation("User {User} initiating seat assignment: Seat {SeatId} -> Passenger {PassengerId} (Booking {BookingId}).",
                user.Identity?.Name, request.SeatId, request.PassengerId, request.BookingId);

            try
            {
                // 1. Retrieve Booking with necessary details
                var booking = await _unitOfWork.Bookings.GetActiveByIdAsync(request.BookingId);
                if (booking == null)
                    return ServiceResult.Failure("Booking not found.");

                // 2. Authorization Check
                var authResult = await AuthorizeBookingAccessAsync(user, booking);
                if (!authResult.IsSuccess)
                    return authResult;

                // 3. Retrieve the Passenger Link
                var bookingPassenger = await _unitOfWork.BookingPassengers.GetActiveByIdAsync(request.BookingId, request.PassengerId);
                if (bookingPassenger == null)
                    return ServiceResult.Failure("Passenger not found within this booking.");

                // 4. Validate Seat, Flight Existence, and FLIGHT STATUS
                var seat = await _unitOfWork.Seats.GetWithCabinClassAsync(request.SeatId);
                var flightInstance = await _unitOfWork.FlightInstances.GetActiveByIdAsync(booking.FlightInstanceId);

                if (seat == null)
                    return ServiceResult.Failure($"Seat ID '{request.SeatId}' not found.");

                if (flightInstance == null)
                    return ServiceResult.Failure("Flight instance associated with booking not found.");

                // --- STRICT FLIGHT STATUS VALIDATION ---

                // Check 1: Status must be 'Scheduled'
                if (!string.Equals(flightInstance.Status, "Scheduled", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Seat assignment denied. Flight {FlightId} is currently '{Status}'.", booking.FlightInstanceId, flightInstance.Status);
                    return ServiceResult.Failure($"Cannot assign seat. Flight status is '{flightInstance.Status}'. Seats can only be modified for Scheduled flights.");
                }

                // Check 2: Time safety net (Cannot modify past flights even if status wasn't updated)
                if (flightInstance.ScheduledDeparture <= DateTime.UtcNow)
                {
                    _logger.LogWarning("Seat assignment denied. Flight {FlightId} has already departed (Time: {Time}).", booking.FlightInstanceId, flightInstance.ScheduledDeparture);
                    return ServiceResult.Failure("Cannot assign seat. The flight has already departed.");
                }
                // --------------------------------------------

                // Check 3: Aircraft Match
                if (seat.AircraftId != flightInstance.AircraftId)
                    return ServiceResult.Failure($"Invalid Seat. Seat '{request.SeatId}' belongs to aircraft '{seat.AircraftId}', but flight is operating on '{flightInstance.AircraftId}'.");

                // 5. Check Availability (Application Level Check)
                // We must ensure this specific seat is not active for ANY passenger on THIS flight instance.
                var isSeatTaken = await _unitOfWork.BookingPassengers.IsSeatAssignedOnFlightAsync(booking.FlightInstanceId, request.SeatId);

                // If seat is taken AND it's not taken by the current passenger (re-assignment case)
                if (isSeatTaken && bookingPassenger.SeatAssignmentId != request.SeatId)
                {
                    _logger.LogWarning("Seat assignment conflict: Seat {SeatId} is already taken on Flight {FlightId}.", request.SeatId, booking.FlightInstanceId);
                    return ServiceResult.Failure($"Seat '{seat.SeatNumber}' is already occupied.");
                }

                // 6. Idempotency Check: If already assigned to this user, do nothing
                if (bookingPassenger.SeatAssignmentId == request.SeatId)
                {
                    return ServiceResult.Success();
                }

                // 7. Persist Changes
                bookingPassenger.SeatAssignmentId = request.SeatId;
                _unitOfWork.BookingPassengers.Update(bookingPassenger);

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Seat {SeatId} successfully assigned to Passenger {PassengerId}.", request.SeatId, request.PassengerId);
                return ServiceResult.Success();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                // 8. Handle Race Conditions (Database Level Check)
                _logger.LogError(ex, "Concurrency error assigning Seat {SeatId} for Booking {BookingId}.", request.SeatId, request.BookingId);
                return ServiceResult.Failure("The selected seat was just taken by another user. Please select a different seat.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error assigning Seat {SeatId} for Booking {BookingId}.", request.SeatId, request.BookingId);
                return ServiceResult.Failure("An internal error occurred while assigning the seat.");
            }
        }

        // Removes the seat assignment for a specific passenger within a booking.
        public async Task<ServiceResult> RemoveSeatAssignmentAsync(int bookingId, int passengerId, ClaimsPrincipal user)
        {
            _logger.LogInformation("User {User} removing seat assignment for PassengerId {PassengerId}, BookingId {BookingId}.", user.Identity?.Name, passengerId, bookingId);

            try
            {
                // 1. Get Booking and verify user access
                var booking = await _unitOfWork.Bookings.GetActiveByIdAsync(bookingId);
                if (booking == null) return ServiceResult.Failure("Booking not found.");
                var authResult = await AuthorizeBookingAccessAsync(user, booking);
                if (!authResult.IsSuccess) return authResult;

                // 2. Get the specific BookingPassenger link
                var bookingPassenger = await _unitOfWork.BookingPassengers.GetActiveByIdAsync(bookingId, passengerId);
                if (bookingPassenger == null)
                    return ServiceResult.Failure("Passenger not found within this booking.");

                // 3. Check if a seat is actually assigned
                if (bookingPassenger.SeatAssignmentId == null)
                { 
                    return ServiceResult.Success();
                }

                // 4. Remove assignment
                bookingPassenger.SeatAssignmentId = null; // Set FK to null
                _unitOfWork.BookingPassengers.Update(bookingPassenger);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully removed seat assignment for PassengerId {PassengerId}, BookingId {BookingId}.", passengerId, bookingId);
                return ServiceResult.Success();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing seat assignment for PassengerId {PassengerId}, BookingId {BookingId}.", passengerId, bookingId);
                return ServiceResult.Failure("An internal error occurred while removing the seat assignment.");
            }
        }

        // Retrieves all current seat assignments for a given booking.
        public async Task<ServiceResult<IEnumerable<SeatAssignmentDto>>> GetSeatAssignmentsForBookingAsync(int bookingId, ClaimsPrincipal user)
        {
            _logger.LogInformation("Retrieving seat assignments for BookingId {BookingId}.", bookingId);
            try
            {
                // 1. Get Booking and verify user access
                var booking = await _unitOfWork.Bookings.GetActiveByIdAsync(bookingId);
                if (booking == null) return ServiceResult<IEnumerable<SeatAssignmentDto>>.Failure("Booking not found.");
                var authResult = await AuthorizeBookingAccessAsync(user, booking);
                if (!authResult.IsSuccess) return ServiceResult<IEnumerable<SeatAssignmentDto>>.Failure(authResult.Errors);

                // 2. Get assignments with details
                var assignments = await _unitOfWork.BookingPassengers.GetAssignmentsByBookingWithDetailsAsync(bookingId); // Needs new repo method

                var dtos = _mapper.Map<IEnumerable<SeatAssignmentDto>>(assignments.Where(bp => bp.SeatAssignmentId != null)); // Filter out unassigned

                return ServiceResult<IEnumerable<SeatAssignmentDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving seat assignments for BookingId {BookingId}.", bookingId);
                return ServiceResult<IEnumerable<SeatAssignmentDto>>.Failure("An error occurred while retrieving seat assignments.");
            }
        }


        // --- Helper Methods ---

        // Duplicated from BookingService - consider moving to a shared helper or base service if common logic increases.
        private async Task<ServiceResult> AuthorizeBookingAccessAsync(ClaimsPrincipal user, Booking booking)
        {
            var appUserId = await _userRepository.GetUserIdFromClaimsPrincipalAsync(user);
            if (string.IsNullOrEmpty(appUserId)) return ServiceResult.Failure("Authentication required.");

            var userProfile = await _unitOfWork.Users.GetUserByAppUserIdAsync(appUserId);
            if (userProfile != null && booking.UserId == userProfile.UserId)
            {
                return ServiceResult.Success(); // Owner
            }

            if (user.IsInRole("Admin") || user.IsInRole("Supervisor") || user.IsInRole("SuperAdmin"))
            {
                return ServiceResult.Success(); // Admin role
            }

            _logger.LogWarning("User {UserId} unauthorized attempt to access Booking ID {BookingId} owned by User ID {OwnerId}.", appUserId, booking.BookingId, booking.UserId);
            return ServiceResult.Failure("Access denied to this booking.");
        }
    }
}
 
