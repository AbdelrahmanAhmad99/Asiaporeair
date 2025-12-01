using Application.DTOs.BoardingPass;
using Application.DTOs.Ticket;  
using Application.Models;
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;  
using Domain.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Application.Services
{
    // Service implementation for managing boarding passes.
    public class BoardingPassService : IBoardingPassService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<BoardingPassService> _logger;
        private readonly IUserRepository _userRepository; 
        private readonly ITicketService _ticketService;   

        // Constructor
        public BoardingPassService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<BoardingPassService> logger,
            IUserRepository userRepository,
            ITicketService ticketService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _userRepository = userRepository;
            _ticketService = ticketService;
        }

        // Generates a boarding pass for a specific passenger on a booking (Check-In process).
        public async Task<ServiceResult<BoardingPassDto>> GenerateBoardingPassAsync(GenerateBoardingPassRequestDto request, ClaimsPrincipal user)
        {
            _logger.LogInformation("Attempting boarding pass generation for BookingId {BookingId}, PassengerId {PassengerId}.", request.BookingId, request.PassengerId);

            // 1. Authorization (User owns booking or is CheckInAgent/Admin)
            var booking = await _unitOfWork.Bookings.GetActiveByIdAsync(request.BookingId);
            if (booking == null) return ServiceResult<BoardingPassDto>.Failure("Booking not found.");
            var authResult = await AuthorizeBookingAccessAsync(user, booking);
            if (!authResult.IsSuccess && !user.IsInRole("CheckInAgent")) // Allow CheckInAgent too
            {
                return ServiceResult<BoardingPassDto>.Failure("Access Denied.");
            }

            // 2. Check if Boarding Pass already exists
            if (await _unitOfWork.BoardingPasses.ExistsForBookingPassengerAsync(request.BookingId, request.PassengerId))
            {
                _logger.LogWarning("Boarding pass already exists for BookingId {BookingId}, PassengerId {PassengerId}.", request.BookingId, request.PassengerId);
                // Optionally retrieve and return the existing one? For now, return failure.
                
                return ServiceResult<BoardingPassDto>.Failure("Boarding pass already exists for this passenger on this booking.");
            }

            // 3. Get BookingPassenger details (includes seat assignment)
            var bookingPassenger = await _unitOfWork.BookingPassengers.GetWithDetailsAsync(request.BookingId, request.PassengerId);
            if (bookingPassenger?.Passenger == null)
                return ServiceResult<BoardingPassDto>.Failure("Passenger not found within this booking.");
            if (bookingPassenger.SeatAssignmentId == null || bookingPassenger.SeatAssignment == null)
                return ServiceResult<BoardingPassDto>.Failure("Seat assignment is required before generating a boarding pass.");

            // 4. Get associated Ticket and validate status
            var ticket = await _unitOfWork.Tickets.GetByBookingAndPassengerAsync(request.BookingId, request.PassengerId);
            if (ticket == null)
                return ServiceResult<BoardingPassDto>.Failure("E-Ticket not found for this passenger and booking.");
            if (ticket.Status != TicketStatus.Issued && ticket.Status != TicketStatus.CheckedIn) // Allow re-issue if already checked-in?
            {
                _logger.LogWarning("Cannot generate boarding pass for TicketId {TicketId} with status {Status}.", ticket.TicketId, ticket.Status);
                return ServiceResult<BoardingPassDto>.Failure($"Cannot generate boarding pass. Ticket status is '{ticket.Status}'.");
            }

            var cabinClass = bookingPassenger.SeatAssignment?.CabinClass?.Name;

            // 2. Define the logic (e.g., Business or First Class get Fast Track)
            bool isEligibleForPrecheck = cabinClass == "Business" || cabinClass == "First" || cabinClass == "Suites";

            // 5. Generate Boarding Pass Entity
            var boardingPass = new BoardingPass
            {
                BookingPassengerBookingId = request.BookingId,
                BookingPassengerPassengerId = request.PassengerId,
                SeatId = bookingPassenger.SeatAssignmentId,
                BoardingTime = CalculateBoardingTime(booking.FlightInstance?.ScheduledDeparture), // Calculate boarding time
                PrecheckStatus = isEligibleForPrecheck, // Determine based on passenger data or rules
                IsDeleted = false
            };

            // 6. Update Ticket Status to CheckedIn (if not already)
            ServiceResult statusUpdateResult = ServiceResult.Success();
            if (ticket.Status != TicketStatus.CheckedIn)
            {
                var statusDto = new UpdateTicketStatusDto { NewStatus = TicketStatus.CheckedIn };
                
                // Pass 'null' for the user, as this is a trusted system action.
                // The BoardingPassService has already authenticated the user's right to the booking.
                statusUpdateResult = await _ticketService.UpdateTicketStatusAsync(ticket.TicketId, statusDto, null);
                if (!statusUpdateResult.IsSuccess)
                {
                    // Fail BP generation if ticket status update fails
                    _logger.LogError("Failed to update ticket status to CheckedIn for TicketId {TicketId} during boarding pass generation. Errors: {Errors}", ticket.TicketId, string.Join("; ", statusUpdateResult.Errors));
                    return ServiceResult<BoardingPassDto>.Failure($"Failed to update ticket status: {string.Join(", ", statusUpdateResult.Errors)}");
                }
                _logger.LogInformation("TicketId {TicketId} status updated to CheckedIn.", ticket.TicketId);
            }


            // --- Transaction Start (Implicit) ---
            try
            {
                await _unitOfWork.BoardingPasses.AddAsync(boardingPass);
                await _unitOfWork.SaveChangesAsync(); // Save BP first to get ID

                _logger.LogInformation("Successfully generated Boarding Pass ID {PassId} for BookingId {BookingId}, PassengerId {PassengerId}.",
                    boardingPass.PassId, request.BookingId, request.PassengerId);

                // --- Transaction Commit (Implicit) ---

                // 7. Map to DTO for response (needs more details)
                var dto = await MapBoardingPassDtoAsync(boardingPass); // Use helper to enrich DTO
                return ServiceResult<BoardingPassDto>.Success(dto);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Boarding Pass for BookingId {BookingId}, PassengerId {PassengerId}.", request.BookingId, request.PassengerId);
                // Attempt rollback? If ticket status was updated, need careful handling.
                return ServiceResult<BoardingPassDto>.Failure("An internal error occurred while saving the boarding pass.");
            }
        }

        // Retrieves a boarding pass by its unique ID.
        public async Task<ServiceResult<BoardingPassDto>> GetBoardingPassByIdAsync(int passId, ClaimsPrincipal user)
        {
            _logger.LogDebug("Retrieving Boarding Pass ID {PassId}.", passId);
            try
            {
                var pass = await _unitOfWork.BoardingPasses.GetActiveByIdAsync(passId); // Simple get by ID
                if (pass == null) return ServiceResult<BoardingPassDto>.Failure("Boarding pass not found.");

                // Authorization check: Requires fetching linked booking
                var booking = await _unitOfWork.Bookings.GetActiveByIdAsync(pass.BookingPassengerBookingId);
                if (booking == null) return ServiceResult<BoardingPassDto>.Failure("Associated booking not found.");
                var authResult = await AuthorizeBookingAccessAsync(user, booking);
                if (!authResult.IsSuccess && !user.IsInRole("CheckInAgent") && !user.IsInRole("GateAgent")) // Allow agents
                {
                    return ServiceResult<BoardingPassDto>.Failure("Access Denied.");
                }

                var dto = await MapBoardingPassDtoAsync(pass);
                return ServiceResult<BoardingPassDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Boarding Pass ID {PassId}.", passId);
                return ServiceResult<BoardingPassDto>.Failure("An internal error occurred.");
            }
        }

        // Retrieves the boarding pass for a specific passenger on a specific booking.
        public async Task<ServiceResult<BoardingPassDto>> GetBoardingPassByBookingPassengerAsync(int bookingId, int passengerId, ClaimsPrincipal user)
        {
            _logger.LogDebug("Retrieving Boarding Pass for BookingId {BookingId}, PassengerId {PassengerId}.", bookingId, passengerId);
            try
            {
                // Authorization (check booking access first)
                var booking = await _unitOfWork.Bookings.GetActiveByIdAsync(bookingId);
                if (booking == null) return ServiceResult<BoardingPassDto>.Failure("Booking not found.");
                var authResult = await AuthorizeBookingAccessAsync(user, booking);
                if (!authResult.IsSuccess && !user.IsInRole("CheckInAgent") && !user.IsInRole("GateAgent"))
                {
                    return ServiceResult<BoardingPassDto>.Failure("Access Denied.");
                }

                // Fetch the pass using the specific repo method
                var pass = await _unitOfWork.BoardingPasses.GetByBookingPassengerAsync(bookingId, passengerId);
                if (pass == null) return ServiceResult<BoardingPassDto>.Failure("Boarding pass not found for this passenger on this booking.");

                var dto = await MapBoardingPassDtoAsync(pass);
                return ServiceResult<BoardingPassDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Boarding Pass for BookingId {BookingId}, PassengerId {PassengerId}.", bookingId, passengerId);
                return ServiceResult<BoardingPassDto>.Failure("An internal error occurred.");
            }
        }

        // Retrieves all boarding passes for a specific flight instance (Gate List/Manifest).
        public async Task<ServiceResult<IEnumerable<BoardingPassDto>>> GetBoardingPassesForFlightAsync(int flightInstanceId)
        {
            _logger.LogInformation("Retrieving boarding passes for FlightInstanceId {FlightId}.", flightInstanceId);
            // No specific user auth here? Assumed internal/agent use. Add if needed.
            try
            {
                var passes = await _unitOfWork.BoardingPasses.GetByFlightInstanceAsync(flightInstanceId); // Repo includes details
                var dtos = new List<BoardingPassDto>();
                foreach (var pass in passes)
                {
                    // Map manually or use helper if needed, AutoMapper might struggle with deep nesting here
                    var dto = await MapBoardingPassDtoAsync(pass); // Use helper for consistency
                    dtos.Add(dto);
                }
                return ServiceResult<IEnumerable<BoardingPassDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving boarding passes for FlightInstanceId {FlightId}.", flightInstanceId);
                return ServiceResult<IEnumerable<BoardingPassDto>>.Failure("An internal error occurred.");
            }
        }

        // Simulates scanning a boarding pass at the gate and updates the ticket status to 'Boarded'.
        public async Task<ServiceResult> ScanBoardingPassAtGateAsync(GateScanRequestDto scanRequest, ClaimsPrincipal gateAgentUser)
        {
            _logger.LogInformation("Gate Agent {User} scanning Boarding Pass ID {PassId} for Flight ID {FlightId}.",
                gateAgentUser.Identity?.Name, scanRequest.PassId, scanRequest.FlightInstanceId);

            // Authorization: Check if user is GateAgent/Admin
            if (!gateAgentUser.IsInRole("GateAgent") && !gateAgentUser.IsInRole("Admin") && !gateAgentUser.IsInRole("SuperAdmin"))
            {
                _logger.LogWarning("Authorization failed: User {User} cannot perform gate scans.", gateAgentUser.Identity?.Name);
                return ServiceResult.Failure("Access Denied. Insufficient permissions for gate operations.");
            }

            try
            {
                // 1. Get Boarding Pass
                var pass = await _unitOfWork.BoardingPasses.GetActiveByIdAsync(scanRequest.PassId);
                if (pass == null) return ServiceResult.Failure("Boarding pass not found.");

                // 2. Get Associated Booking/Flight to verify
                var bookingPassenger = await _unitOfWork.BookingPassengers.GetWithDetailsAsync(pass.BookingPassengerBookingId, pass.BookingPassengerPassengerId); // Includes Passenger, Booking
                if (bookingPassenger?.Booking == null) return ServiceResult.Failure("Associated booking or passenger not found.");
  
                // 3. Verify Flight Match
                if (bookingPassenger.Booking.FlightInstanceId != scanRequest.FlightInstanceId)
                {
                    _logger.LogWarning("Gate Scan Failed: Boarding Pass {PassId} is for Flight {ActualFlightId}, but scanned at gate for Flight {ScannedFlightId}.",
                        scanRequest.PassId, bookingPassenger.Booking.FlightInstanceId, scanRequest.FlightInstanceId);
                    return ServiceResult.Failure("Boarding pass is not valid for this flight.");
                }

                // 4. Get Ticket and Check Status (Should be CheckedIn)
                var ticket = await _unitOfWork.Tickets.GetByBookingAndPassengerAsync(pass.BookingPassengerBookingId, pass.BookingPassengerPassengerId);
                if (ticket == null) return ServiceResult.Failure("Associated e-ticket not found.");

                if (ticket.Status == TicketStatus.Boarded)
                {
                    _logger.LogWarning("Gate Scan: Ticket ID {TicketId} (Pass ID {PassId}) already marked as Boarded.", ticket.TicketId, pass.PassId);
                   
                    return ServiceResult.Success();
                }
                if (ticket.Status != TicketStatus.CheckedIn)
                {
                    _logger.LogWarning("Gate Scan Failed: Ticket ID {TicketId} (Pass ID {PassId}) has status {Status}, expected CheckedIn.", ticket.TicketId, pass.PassId, ticket.Status);
                    return ServiceResult.Failure($"Invalid ticket status ('{ticket.Status}'). Passenger must be checked-in.");
                }

                // 5. Update Ticket Status to Boarded
                var statusDto = new UpdateTicketStatusDto { NewStatus = TicketStatus.Boarded };
                var statusResult = await _ticketService.UpdateTicketStatusAsync(ticket.TicketId, statusDto, gateAgentUser); // Pass agent user

                if (!statusResult.IsSuccess)
                {
                    // UpdateTicketStatusAsync logs errors
                    return ServiceResult.Failure($"Failed to update ticket status to Boarded: {string.Join(", ", statusResult.Errors)}");
                }

                // 6. Optionally Update BoardingPass BoardingTime
                if (!pass.BoardingTime.HasValue) // Only set if not already set (e.g., if re-scanned)
                {
                    pass.BoardingTime = DateTime.UtcNow;
                    _unitOfWork.BoardingPasses.Update(pass);
                    await _unitOfWork.SaveChangesAsync(); // Save boarding time update
                }

                _logger.LogInformation("Successfully processed gate scan for Pass ID {PassId}. Ticket ID {TicketId} status set to Boarded.", pass.PassId, ticket.TicketId);
              
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing gate scan for Pass ID {PassId}, Flight ID {FlightId}.", scanRequest.PassId, scanRequest.FlightInstanceId);
                return ServiceResult.Failure("An internal error occurred during gate scan processing.");
            }
        }

        // Performs a paginated search for boarding passes (Admin/Support).
        public async Task<ServiceResult<PaginatedResult<BoardingPassDto>>> SearchBoardingPassesAsync(BoardingPassFilterDto filter, int pageNumber, int pageSize)
        {
            _logger.LogInformation("Searching boarding passes page {PageNumber}.", pageNumber);
            // Auth check? Assume admin access required. Add if needed.
            try
            {
                // Build filter expression - Requires joins, best handled in Repository
                Expression<Func<BoardingPass, bool>> filterExpression = bp => (filter.IncludeDeleted || !bp.IsDeleted);
                if (filter.FlightInstanceId.HasValue) filterExpression = filterExpression.And(bp => bp.BookingPassenger.Booking.FlightInstanceId == filter.FlightInstanceId.Value); // Needs Includes
                if (!string.IsNullOrWhiteSpace(filter.SeatNumber)) filterExpression = filterExpression.And(bp => bp.Seat.SeatNumber == filter.SeatNumber); // Needs Includes
                if (!string.IsNullOrWhiteSpace(filter.PassengerNameContains))
                {
                    var name = filter.PassengerNameContains.ToLower();
                    filterExpression = filterExpression.And(bp => (bp.BookingPassenger.Passenger.FirstName.ToLower() + " " + bp.BookingPassenger.Passenger.LastName.ToLower()).Contains(name)); // Needs Includes
                }
                // Filtering by HasBoarded requires joining with Ticket status - complex query

                // Use generic GetPagedAsync with Includes (or dedicated repo search method)
                var (items, totalCount) = await _unitOfWork.BoardingPasses.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    filterExpression,
                    orderBy: q => q.OrderBy(bp => bp.BookingPassengerBookingId).ThenBy(bp => bp.BookingPassengerPassengerId),
                   includeProperties: "BookingPassenger.Booking.FlightInstance.Schedule.Route,BookingPassenger.Passenger,Seat.CabinClass" // Specify needed includes
                );

                var dtos = new List<BoardingPassDto>();
                foreach (var pass in items)
                {
                    dtos.Add(await MapBoardingPassDtoAsync(pass)); // Use helper to map
                }

                var paginatedResult = new PaginatedResult<BoardingPassDto>(dtos, totalCount, pageNumber, pageSize);
                return ServiceResult<PaginatedResult<BoardingPassDto>>.Success(paginatedResult);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching boarding passes.");
                return ServiceResult<PaginatedResult<BoardingPassDto>>.Failure("An error occurred during search.");
            }
        }

        // Soft-deletes a boarding pass.
        public async Task<ServiceResult> VoidBoardingPassAsync(int passId, ClaimsPrincipal performingUser)
        {
            _logger.LogInformation("User {User} voiding Boarding Pass ID {PassId}.", performingUser.Identity?.Name, passId);
            // Authorization: Admin/SuperAdmin or CheckInAgent?
            if (!performingUser.IsInRole("Admin") && !performingUser.IsInRole("SuperAdmin") && !performingUser.IsInRole("CheckInAgent"))
            {
                _logger.LogWarning("Authorization failed: User {User} cannot void boarding passes.", performingUser.Identity?.Name);
                return ServiceResult.Failure("Access Denied.");
            }

            var pass = await _unitOfWork.BoardingPasses.GetActiveByIdAsync(passId);
            if (pass == null) return ServiceResult.Failure("Boarding pass not found.");

            // Get related Ticket to potentially revert status
            var ticket = await _unitOfWork.Tickets.GetByBookingAndPassengerAsync(pass.BookingPassengerBookingId, pass.BookingPassengerPassengerId);

            // Validation: Cannot void if already boarded?
            if (ticket?.Status == TicketStatus.Boarded)
            {
                _logger.LogWarning("Voiding failed for Pass ID {PassId}: Passenger already boarded.", passId);
                return ServiceResult.Failure("Cannot void boarding pass after passenger has boarded.");
            }

            try
            {
                _unitOfWork.BoardingPasses.SoftDelete(pass);

                // Revert Ticket status from CheckedIn back to Issued? (Business rule)
                if (ticket != null && ticket.Status == TicketStatus.CheckedIn)
                {
                    _logger.LogInformation("Reverting Ticket ID {TicketId} status to Issued after voiding Pass ID {PassId}.", ticket.TicketId, passId);
                    var statusDto = new UpdateTicketStatusDto { NewStatus = TicketStatus.Issued };
                    // Pass performing user for audit trail on ticket status change
                    var ticketUpdateResult = await _ticketService.UpdateTicketStatusAsync(ticket.TicketId, statusDto, performingUser);
                    if (!ticketUpdateResult.IsSuccess)
                    {
                        // Log error but proceed with voiding the pass itself? Or fail? Fail for safety.
                        _logger.LogError("Failed to revert ticket status for Ticket ID {TicketId} while voiding Pass ID {PassId}. Errors: {Errors}", ticket.TicketId, passId, string.Join("; ", ticketUpdateResult.Errors));
                        // Don't save changes if ticket update failed
                        return ServiceResult.Failure($"Failed to revert ticket status: {string.Join(", ", ticketUpdateResult.Errors)}");
                    }
                }

                await _unitOfWork.SaveChangesAsync(); // Save BP deletion and potentially ticket status update
                _logger.LogInformation("Successfully voided Boarding Pass ID {PassId}.", passId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error voiding Boarding Pass ID {PassId}.", passId);
                return ServiceResult.Failure("An internal error occurred.");
            }
        }

        // --- Helper Methods ---

        // Calculates boarding time based on departure time.
        private DateTime CalculateBoardingTime(DateTime? departureTime)
        {
            // Simple rule: Boarding starts 45 minutes before departure.
            // Add more complex rules based on aircraft, airport, etc. if needed.
            return departureTime?.AddMinutes(-45) ?? DateTime.UtcNow.AddMinutes(15); // Fallback
        }

        // Maps BoardingPass entity to DTO, enriching with related data.
        private async Task<BoardingPassDto> MapBoardingPassDtoAsync(BoardingPass pass)
        {
            // Ensure necessary related data is loaded (might need explicit loading if not included)
            if (pass.BookingPassenger == null)
                pass.BookingPassenger = await _unitOfWork.BookingPassengers.GetWithDetailsAsync(pass.BookingPassengerBookingId, pass.BookingPassengerPassengerId); // Includes Passenger, Booking
            if (pass.Seat == null)
                pass.Seat = await _unitOfWork.Seats.GetWithCabinClassAsync(pass.SeatId);
            if (pass.BookingPassenger?.Booking == null) // Load booking if missing
                pass.BookingPassenger.Booking = await _unitOfWork.Bookings.GetActiveByIdAsync(pass.BookingPassengerBookingId);
            if (pass.BookingPassenger?.Booking?.FlightInstance == null) // Load flight instance if missing
                pass.BookingPassenger.Booking.FlightInstance = await _unitOfWork.FlightInstances.GetWithDetailsAsync(pass.BookingPassenger.Booking.FlightInstanceId);
            if (pass.BookingPassenger?.Passenger?.User == null) // Load user if missing
            {
                
                // Option 1: If Passenger only links to User (int UserId), fetch User then AppUser
                var userProfile = await _unitOfWork.Users.GetUserProfileByIdAsync(pass.BookingPassenger.Passenger.UserId); // Get User profile by int ID
                if (userProfile != null)
                {
                    pass.BookingPassenger.Passenger.User = userProfile; // Assign the fetched User profile
                    if (userProfile.AppUser == null) // Check if AppUser was included
                    {
                        userProfile.AppUser = await _userRepository.GetByIdAsync(userProfile.AppUserId); // Fetch AppUser by string ID
                    }
                }
            }

            var dto = _mapper.Map<BoardingPassDto>(pass); // Use AutoMapper for base mapping

            // Manual mapping/enrichment for complex fields
            dto.PassengerId = pass.BookingPassengerPassengerId;
            dto.PassengerName = $"{pass.BookingPassenger?.Passenger?.FirstName} {pass.BookingPassenger?.Passenger?.LastName}";
            dto.FrequentFlyerNumber = pass.BookingPassenger?.Passenger?.User?.FrequentFlyer?.CardNumber ?? "N/A"; // Requires includes
            dto.FlightInstanceId = pass.BookingPassenger?.Booking?.FlightInstanceId ?? 0;
            dto.FlightNumber = pass.BookingPassenger?.Booking?.FlightInstance?.Schedule?.FlightNo ?? "N/A";
            dto.OriginAirportCode = pass.BookingPassenger?.Booking?.FlightInstance?.Schedule?.Route?.OriginAirportId ?? "N/A";
            dto.DestinationAirportCode = pass.BookingPassenger?.Booking?.FlightInstance?.Schedule?.Route?.DestinationAirportId ?? "N/A";
            dto.DepartureTime = pass.BookingPassenger?.Booking?.FlightInstance?.ScheduledDeparture ?? default;
            dto.ArrivalTime = pass.BookingPassenger?.Booking?.FlightInstance?.ScheduledArrival ?? default;
            dto.SeatNumber = pass.Seat?.SeatNumber ?? "N/A";
            dto.CabinClass = pass.Seat?.CabinClass?.Name ?? "N/A";
            dto.BoardingTime = pass.BoardingTime ?? CalculateBoardingTime(dto.DepartureTime); // Recalculate if null?
            dto.BookingReference = pass.BookingPassenger?.Booking?.BookingRef ?? "N/A";

            // Get Ticket Code
            var ticket = await _unitOfWork.Tickets.GetByBookingAndPassengerAsync(pass.BookingPassengerBookingId, pass.BookingPassengerPassengerId);
            dto.TicketCode = ticket?.TicketCode ?? "N/A";

            // Sequence Number (Example: based on Passenger ID) - needs better logic
            dto.SequenceNumber = (pass.BookingPassengerPassengerId % 100) + 1;

            // Gate Information (Needs to be fetched from operational data, maybe FlightInstance needs Gate property?)
            // dto.Gate = flightInstance.DepartureGate; // Example if FlightInstance had gate info

            return dto;
        }

        // Authorization helper - Refactor needed.
        private async Task<ServiceResult> AuthorizeBookingAccessAsync(ClaimsPrincipal user, Booking booking)
        {
            var appUserId = await _userRepository.GetUserIdFromClaimsPrincipalAsync(user);
            if (string.IsNullOrEmpty(appUserId)) return ServiceResult.Failure("Authentication required.");
            var userProfile = await _unitOfWork.Users.GetUserByAppUserIdAsync(appUserId);
            if (userProfile != null && booking.UserId == userProfile.UserId) return ServiceResult.Success();
            if (user.IsInRole("Admin") || user.IsInRole("Supervisor") || user.IsInRole("SuperAdmin")) return ServiceResult.Success();
            _logger.LogWarning("User {UserId} unauthorized attempt to access Booking ID {BookingId} owned by User ID {OwnerId}.", appUserId, booking.BookingId, booking.UserId);
            return ServiceResult.Failure("Access denied to this booking.");
        }
    }
}