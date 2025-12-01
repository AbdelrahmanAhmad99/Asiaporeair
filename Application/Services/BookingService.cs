using Application.DTOs.Booking;
using Application.DTOs.Passenger; // Needed for CreatePassengerDto
using Application.Models;
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums; // Assuming BookingStatus enum is here if used
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
    // Service implementation for managing the booking lifecycle.
    public class BookingService : IBookingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPricingService _pricingService;
        private readonly IPassengerService _passengerService; // Passenger creation/linking logic
        private readonly ILogger<BookingService> _logger;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository; // To get User ID

        public BookingService(
            IUnitOfWork unitOfWork,
            IPricingService pricingService,
            IPassengerService passengerService,
            ILogger<BookingService> logger,
            IMapper mapper,
            IUserRepository userRepository)
        {
            _unitOfWork = unitOfWork;
            _pricingService = pricingService;
            _passengerService = passengerService;
            _logger = logger;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        // Creates a new booking with strict validation on flight status, user ownership, and ancillary product existence.
        public async Task<ServiceResult<BookingDto>> CreateBookingAsync(CreateBookingDto createDto, ClaimsPrincipal user)
        {
            _logger.LogInformation("Initiating booking creation for FlightInstanceId {FlightId}.", createDto.FlightInstanceId);

            // 1. Security: Authenticate and retrieve the acting User Profile.
            // We do not trust the input; we strictly use the ClaimsPrincipal.
            var appUserId = await _userRepository.GetUserIdFromClaimsPrincipalAsync(user);
            if (string.IsNullOrEmpty(appUserId))
                return ServiceResult<BookingDto>.Failure("User is not authenticated.");

            var userProfile = await _unitOfWork.Users.GetUserByAppUserIdAsync(appUserId);
            if (userProfile == null)
                return ServiceResult<BookingDto>.Failure("Customer profile not found for the current user.");

            int bookingUserId = userProfile.UserId; // This is the Verified ID from the token.

            // 2. Validation: Flight Instance Existence and Status.
            var flightInstance = await _unitOfWork.FlightInstances.GetWithDetailsAsync(createDto.FlightInstanceId);
            if (flightInstance == null)
                return ServiceResult<BookingDto>.Failure("Flight instance not found.");

            // STRICT STATUS CHECK: Ensure the flight is 'Scheduled'. 
            // We reject 'Cancelled', 'Departed', or 'Arrived' flights.
            if (!string.Equals(flightInstance.Status, "Scheduled", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Booking failed. Flight {FlightId} status is '{Status}'.", createDto.FlightInstanceId, flightInstance.Status);
                return ServiceResult<BookingDto>.Failure($"Cannot book this flight. Current status: {flightInstance.Status}.");
            }

            // TIME CHECK: Ensure the flight has not already departed.
            if (flightInstance.ScheduledDeparture <= DateTime.UtcNow)
            {
                return ServiceResult<BookingDto>.Failure("Cannot book a flight that has already departed.");
            }

            // 3. Validation: Seat Availability.
            // We calculate availability based on the aircraft's capacity minus confirmed bookings.
            var (totalCapacity, bookedSeats) = await _unitOfWork.FlightInstances.GetSeatCountsAsync(createDto.FlightInstanceId);
            int availableSeats = totalCapacity - bookedSeats;

            if (availableSeats < createDto.Passengers.Count)
            {
                return ServiceResult<BookingDto>.Failure($"Insufficient seats. Only {availableSeats} remaining.");
            }

            // 4. Logic: Calculate Total Booking Price.
            // This service handles the complex math for base fares and passenger types.
            var priceResult = await _pricingService.CalculateBookingPriceAsync(createDto);
            if (!priceResult.IsSuccess)
                return ServiceResult<BookingDto>.Failure(priceResult.Errors);

            // 5. Transactional Logic (The Core Operation)
            // We use an explicit transaction to ensure Atomicity. 
            // If Ancillary validation fails, we rollback the Booking and Passengers.
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // A. Create Booking Header (The Parent Entity)
                var booking = new Booking
                {
                    UserId = bookingUserId, // Secured User ID from Token
                    FlightInstanceId = createDto.FlightInstanceId,
                    BookingRef = GenerateBookingReference(),
                    BookingTime = DateTime.UtcNow,
                    PriceTotal = priceResult.Data,
                    PaymentStatus = "Pending",
                    FareBasisCodeId = createDto.FareBasisCode,
                    IsDeleted = false
                };

                await _unitOfWork.Bookings.AddAsync(booking);
                await _unitOfWork.SaveChangesAsync(); // Save immediately to generate the BookingId for relationships

                _logger.LogInformation("Booking Header created. ID: {BookingId}, Ref: {Ref}", booking.BookingId, booking.BookingRef);

                // B. Process Passengers
                // Map Input DTO to CreatePassengerDto, strictly enforcing the UserId to prevent IDOR.
                var passengerDtos = createDto.Passengers.Select(p => new CreatePassengerDto
                {
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    DateOfBirth = p.DateOfBirth,
                    PassportNumber = p.PassportNumber,
                    //FrequentFlyerCardNumber = p.FrequentFlyerCardNumber,
                    UserId = bookingUserId // Force Link to Current User
                }).ToList();

                var passengerResult = await _passengerService.AddMultiplePassengersAsync(passengerDtos, booking.BookingId);

                if (!passengerResult.IsSuccess)
                {
                    // If passenger creation fails, we must fail the whole booking.
                    throw new Exception($"Passenger creation failed: {string.Join(", ", passengerResult.Errors)}");
                }

                // C. Process Ancillaries (FIXED LOGIC)
                // We iterate through requested items and validate they exist in the DB.
                if (createDto.AncillaryPurchases != null && createDto.AncillaryPurchases.Any())
                {
                    var ancillarySales = new List<AncillarySale>();

                    foreach (var purchase in createDto.AncillaryPurchases)
                    {
                        // Check 1: Quantity Validation
                        if (purchase.Quantity <= 0) continue; // Skip invalid quantities, or throw error if strict

                        // Check 2: Database Existence Check
                        // We fetch the product to ensure it exists AND is active (not deleted).
                        var product = await _unitOfWork.AncillaryProducts.GetActiveByIdAsync(purchase.ProductId);

                        if (product == null)
                        {
                            // CRITICAL FIX: If product 0 (or any invalid ID) is requested, we stop everything.
                            // We do not want to create a booking if the user's paid add-ons cannot be fulfilled.
                            _logger.LogWarning("Booking failed: Ancillary Product ID {ProductId} not found.", purchase.ProductId);

                            // Explicitly rollback and return failure
                            await transaction.RollbackAsync();
                            return ServiceResult<BookingDto>.Failure($"Ancillary Product with ID {purchase.ProductId} does not exist or is no longer available.");
                        }

                        // Create the Sale Record
                        // Note: We use the Price from the Database (product.BaseCost), not user input, for security.
                        ancillarySales.Add(new AncillarySale
                        {
                            BookingId = booking.BookingId,
                            ProductId = purchase.ProductId,
                            Quantity = purchase.Quantity,
                            PricePaid = (product.BaseCost ?? 0) * purchase.Quantity, // Snapshot the price
                            SegmentId = purchase.SegmentId,
                            IsDeleted = false
                        });
                    }

                    // Batch insert valid sales
                    if (ancillarySales.Any())
                    {
                        await _unitOfWork.AncillarySales.AddRangeAsync(ancillarySales);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }

                // D. Commit Transaction
                // If we reached here, Flight, Passengers, and Ancillaries are all valid.
                await transaction.CommitAsync();
                _logger.LogInformation("Transaction committed successfully for Booking {Ref}.", booking.BookingRef);

                // 6. Final Data Retrieval (The "Reload" Step)
                // We clear the ChangeTracker to force a fresh fetch from the DB. 
                // This guarantees the returned DTO contains the actual generated IDs (PassengerId, LinkedUserId, etc.).
                _unitOfWork.ClearChangeTracker();

                var finalBooking = await _unitOfWork.Bookings.GetWithDetailsAsync(booking.BookingId);

                // Fallback: Manually load passengers if the Repository method didn't include the deep graph
                if (finalBooking.BookingPassengers == null || !finalBooking.BookingPassengers.Any())
                {
                    var passengers = await _unitOfWork.BookingPassengers.GetByBookingAsync(booking.BookingId);
                    finalBooking.BookingPassengers = passengers.ToList();
                }

                var resultDto = _mapper.Map<BookingDto>(finalBooking);

                // Ensure Passengers are mapped correctly from the reloaded entity
                if (resultDto.Passengers == null || !resultDto.Passengers.Any())
                {
                    resultDto.Passengers = finalBooking.BookingPassengers
                        .Select(bp => _mapper.Map<PassengerDto>(bp.Passenger))
                        .ToList();
                }

                return ServiceResult<BookingDto>.Success(resultDto);
            }
            catch (Exception ex)
            {
                // Catch-all for database errors, network issues, or our custom Exceptions thrown above.
                // We strictly rollback to ensure no partial data (orphan bookings) exists.
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Critical error during CreateBooking transaction for Flight {FlightId}.", createDto.FlightInstanceId);
                return ServiceResult<BookingDto>.Failure($"Booking failed due to an internal error: {ex.Message}");
            }
        }

        // Helper to generate a 6-character alphanumeric booking reference (e.g., "7362A0")
        private string GenerateBookingReference()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
        }

        // Retrieves a booking summary by its ID.
        public async Task<ServiceResult<BookingDto>> GetBookingSummaryByIdAsync(int bookingId, ClaimsPrincipal user)
        {
            var booking = await _unitOfWork.Bookings.GetActiveByIdAsync(bookingId); // Less detailed query
            if (booking == null) return ServiceResult<BookingDto>.Failure("Booking not found.");

            // Authorization
            var authResult = await AuthorizeBookingAccessAsync(user, booking);
            if (!authResult.IsSuccess) return ServiceResult<BookingDto>.Failure(authResult.Errors);

            // Need Flight Number etc. - Requires loading related data
            if (booking.FlightInstance == null)
                booking.FlightInstance = await _unitOfWork.FlightInstances.GetWithDetailsAsync(booking.FlightInstanceId);
            if (booking.BookingPassengers == null || !booking.BookingPassengers.Any()) // Fetch passengers if not included
            {
                var passengers = await _passengerService.GetPassengersByBookingAsync(bookingId);
                if (!passengers.IsSuccess) _logger.LogWarning("Could not retrieve passengers for Booking Summary {BookingId}", bookingId);
                booking.BookingPassengers = passengers.Data?.Select(p => new BookingPassenger { PassengerId = p.PassengerId, Passenger = _mapper.Map<Passenger>(p) }).ToList() ?? new List<BookingPassenger>();
            }

            var dto = _mapper.Map<BookingDto>(booking);
            dto.Passengers = _mapper.Map<List<PassengerDto>>(booking.BookingPassengers.Select(bp => bp.Passenger));

            return ServiceResult<BookingDto>.Success(dto);
        }

        // Retrieves detailed booking information by ID.
        public async Task<ServiceResult<BookingDetailDto>> GetBookingDetailsByIdAsync(int bookingId, ClaimsPrincipal user)
        {
            _logger.LogDebug("Retrieving details for Booking ID {BookingId}.", bookingId);
            var booking = await _unitOfWork.Bookings.GetWithDetailsAsync(bookingId); // Repo method includes everything
            if (booking == null) return ServiceResult<BookingDetailDto>.Failure("Booking not found.");

            // Authorization
            var authResult = await AuthorizeBookingAccessAsync(user, booking);
            if (!authResult.IsSuccess) return ServiceResult<BookingDetailDto>.Failure(authResult.Errors);

            var dto = _mapper.Map<BookingDetailDto>(booking);
            return ServiceResult<BookingDetailDto>.Success(dto);
        }

        // Retrieves detailed booking information by reference code.
        public async Task<ServiceResult<BookingDetailDto>> GetBookingDetailsByReferenceAsync(string bookingReference, ClaimsPrincipal user)
        {
            _logger.LogDebug("Retrieving details for Booking Reference {BookingRef}.", bookingReference);
            // Need a GetWithDetailsByReferenceAsync in repo or load details manually
            var booking = await _unitOfWork.Bookings.GetByReferenceAsync(bookingReference);
            if (booking == null) return ServiceResult<BookingDetailDto>.Failure("Booking not found.");

            // Reload with full details
            var detailedBooking = await _unitOfWork.Bookings.GetWithDetailsAsync(booking.BookingId);
            if (detailedBooking == null) return ServiceResult<BookingDetailDto>.Failure("Booking found by reference, but failed to load details.");


            // Authorization
            var authResult = await AuthorizeBookingAccessAsync(user, detailedBooking);
            if (!authResult.IsSuccess) return ServiceResult<BookingDetailDto>.Failure(authResult.Errors);

            var dto = _mapper.Map<BookingDetailDto>(detailedBooking);
            return ServiceResult<BookingDetailDto>.Success(dto);
        }

        // Retrieves all bookings for the currently authenticated user (paginated).
        public async Task<ServiceResult<PaginatedResult<BookingDto>>> GetMyBookingsAsync(ClaimsPrincipal user, int pageNumber, int pageSize)
        {
            var appUserId = await _userRepository.GetUserIdFromClaimsPrincipalAsync(user);
            if (string.IsNullOrEmpty(appUserId)) return ServiceResult<PaginatedResult<BookingDto>>.Failure("Authentication required.");
            var userProfile = await _unitOfWork.Users.GetUserByAppUserIdAsync(appUserId);
            if (userProfile == null) return ServiceResult<PaginatedResult<BookingDto>>.Failure("User profile not found.");

            _logger.LogInformation("Retrieving bookings for User ID {UserId}, Page {PageNumber}.", userProfile.UserId, pageNumber);
            try
            { 
                // Workaround: Filter all user bookings then paginate in memory (inefficient)
                var allUserBookings = await _unitOfWork.Bookings.GetByUserIdAsync(userProfile.UserId); // Assumes this method exists

                var totalCount = allUserBookings.Count();
                var pagedBookings = allUserBookings
                    .OrderByDescending(b => b.BookingTime)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Map results (need passengers for DTO)
                var dtos = new List<BookingDto>();
                foreach (var booking in pagedBookings)
                {
                    // Manually load passengers if GetByUserIdAsync doesn't include them
                    if (booking.BookingPassengers == null || !booking.BookingPassengers.Any())
                    {
                        var passengers = await _passengerService.GetPassengersByBookingAsync(booking.BookingId);
                        booking.BookingPassengers = passengers.Data?.Select(p => new BookingPassenger { PassengerId = p.PassengerId, Passenger = _mapper.Map<Passenger>(p) }).ToList() ?? new List<BookingPassenger>();
                    }
                    var dto = _mapper.Map<BookingDto>(booking);
                    dtos.Add(dto);
 
                }

                var paginatedResult = new PaginatedResult<BookingDto>(dtos, totalCount, pageNumber, pageSize);
                return ServiceResult<PaginatedResult<BookingDto>>.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookings for User ID {UserId}.", userProfile.UserId);
                return ServiceResult<PaginatedResult<BookingDto>>.Failure("An error occurred while retrieving bookings.");
            }
        }

        // Updates the payment status of a booking.
        public async Task<ServiceResult> UpdateBookingPaymentStatusAsync(int bookingId, UpdateBookingStatusDto statusDto, ClaimsPrincipal? performingUser = null)
        {
            _logger.LogInformation("Updating payment status for Booking ID {BookingId} to {Status}. Performed by: {User}",
                bookingId, statusDto.NewStatus, performingUser?.Identity?.Name ?? "System");
 
            try
            {
                var success = await _unitOfWork.Bookings.UpdatePaymentStatusAsync(bookingId, statusDto.NewStatus);
                if (!success)
                {
                    _logger.LogWarning("Failed to update payment status for Booking ID {BookingId}: Booking not found.", bookingId);
                    return ServiceResult.Failure("Booking not found.");
                }

                await _unitOfWork.SaveChangesAsync();

                // Post-Confirmation Actions (if status is 'Confirmed')
                if (statusDto.NewStatus.Equals("Confirmed", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Booking {BookingId} confirmed. Triggering ticket generation.", bookingId);
                    // Ideally, trigger an event or call ITicketService.GenerateTicketsForBookingAsync(bookingId);
                    // Placeholder: Log for now
                }
                // Post-Cancellation Actions (if status is 'Cancelled')
                else if (statusDto.NewStatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogInformation("Booking {BookingId} cancelled. Reason: {Reason}", bookingId, statusDto.Reason ?? "N/A");
                    // Trigger refund process? Release seats?
                }

                _logger.LogInformation("Successfully updated payment status for Booking ID {BookingId} to {Status}.", bookingId, statusDto.NewStatus);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for Booking ID {BookingId}.", bookingId);
                return ServiceResult.Failure($"An error occurred: {ex.Message}");
            }
        }

        // Cancels a booking.
        public async Task<ServiceResult> CancelBookingAsync(int bookingId, ClaimsPrincipal user, string? reason = null)
        {
            _logger.LogInformation("User {User} attempting to cancel Booking ID {BookingId}. Reason: {Reason}", user.Identity?.Name, bookingId, reason ?? "User request");
            var booking = await _unitOfWork.Bookings.GetWithDetailsAsync(bookingId); // Need details for validation/auth
            if (booking == null) return ServiceResult.Failure("Booking not found.");

            // Authorization
            var authResult = await AuthorizeBookingAccessAsync(user, booking);
            if (!authResult.IsSuccess) return ServiceResult.Failure(authResult.Errors);

            // Validation: Check current status, time before flight, etc.
            if (booking.PaymentStatus == "Cancelled") return ServiceResult.Failure("Booking is already cancelled.");
            if (booking.FlightInstance != null && booking.FlightInstance.ScheduledDeparture < DateTime.UtcNow.AddHours(2)) // Example: Cannot cancel within 2 hours of departure
            {
                _logger.LogWarning("Cancellation denied for Booking ID {BookingId}: Too close to departure.", bookingId);
                return ServiceResult.Failure("Cannot cancel booking too close to the flight departure time.");
            }

            // Perform cancellation via status update
            var statusDto = new UpdateBookingStatusDto { NewStatus = "Cancelled", Reason = reason ?? "User request" };
            var updateResult = await UpdateBookingPaymentStatusAsync(bookingId, statusDto, user); // Pass user for logging/audit

            if (!updateResult.IsSuccess)
            {
                // UpdateBookingPaymentStatusAsync already logged the error
                return ServiceResult.Failure($"Failed to update booking status to Cancelled: {string.Join(", ", updateResult.Errors)}");
            }

            // Additional cancellation logic (e.g., void tickets, process refund based on fare rules) would go here
            _logger.LogInformation("Booking ID {BookingId} cancellation process initiated successfully.", bookingId);
            return ServiceResult.Success();
        }
         
        // Retrieves a simplified list of passengers for a specific flight instance (Passenger Manifest).
        public async Task<ServiceResult<IEnumerable<object>>> GetPassengerManifestForFlightAsync(int flightInstanceId)
        {
            _logger.LogInformation("Generating passenger manifest for FlightInstanceId {FlightId}.", flightInstanceId);
            if (!await _unitOfWork.FlightInstances.AnyAsync(fi => fi.InstanceId == flightInstanceId))
                return ServiceResult<IEnumerable<object>>.Failure("Flight instance not found.");

            try
            { 
                var confirmedBookings = await _unitOfWork.Bookings.GetByFlightInstanceAsync(flightInstanceId);
                confirmedBookings = confirmedBookings.Where(b => b.PaymentStatus == "Confirmed");

                var manifest = new List<object>();

                foreach (var booking in confirmedBookings)
                {
                    foreach (var bp in booking.BookingPassengers)
                    {
                        if (bp.Passenger != null)
                        {
                            manifest.Add(new
                            {
                                PassengerId = bp.PassengerId,
                                FullName = $"{bp.Passenger.FirstName} {bp.Passenger.LastName}",
                                PassportNumber = bp.Passenger.PassportNumber, 
                                AssignedSeat = bp.SeatAssignment?.SeatNumber ?? "N/A",
                                BookingRef = booking.BookingRef, 
                                FrequentFlyerNumber = booking.User?.FrequentFlyer?.CardNumber  
                            });
                        }
                    }
                }
                _logger.LogInformation("Generated manifest with {Count} passengers for FlightInstanceId {FlightId}.", manifest.Count, flightInstanceId);
                return ServiceResult<IEnumerable<object>>.Success(manifest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating manifest for FlightInstanceId {FlightId}.", flightInstanceId);
                return ServiceResult<IEnumerable<object>>.Failure("An error occurred while generating the passenger manifest.");
            }
        }

        // --- Helper Methods ---

        // Centralized authorization check for accessing booking data.
        private async Task<ServiceResult> AuthorizeBookingAccessAsync(ClaimsPrincipal user, Booking booking)
        {
            var appUserId = await _userRepository.GetUserIdFromClaimsPrincipalAsync(user);
            if (string.IsNullOrEmpty(appUserId)) return ServiceResult.Failure("Authentication required.");

            // Check ownership
            var userProfile = await _unitOfWork.Users.GetUserByAppUserIdAsync(appUserId);
            if (userProfile != null && booking.UserId == userProfile.UserId)
            {
                return ServiceResult.Success(); // Owner access granted
            }

            // Check admin roles
            if (user.IsInRole("Admin") || user.IsInRole("Supervisor") || user.IsInRole("SuperAdmin"))
            {
                return ServiceResult.Success(); // Admin access granted
            }

            _logger.LogWarning("User {UserId} unauthorized attempt to access Booking ID {BookingId} owned by User ID {OwnerId}.", appUserId, booking.BookingId, booking.UserId);
            return ServiceResult.Failure("Access denied to this booking.");
        }

        public async Task<ServiceResult<BookingDto>> CreateBookingAsync(CreateBookingDto createDto, string userId)
        {
            // 1. Validate user and get the related 'User' entity
            var appUser = await _unitOfWork.Users.GetByIdAsync(userId);
            if (appUser?.User == null)
            {
                return ServiceResult<BookingDto>.Failure("User not found or is not a customer.");
            }

            // 2. Validate flight instance
            var flightInstance = await _unitOfWork.FlightInstances.GetWithDetailsAsync(createDto.FlightInstanceId);
            if (flightInstance == null)
            {
                return ServiceResult<BookingDto>.Failure("Flight not found.");
            }

            // 3. Calculate total price dynamically based on all factors
            var totalPriceResult = await _pricingService.CalculateBookingPriceAsync(createDto);
            if (!totalPriceResult.IsSuccess)
            {
                return ServiceResult<BookingDto>.Failure(totalPriceResult.Errors);
            }
            var totalPrice = totalPriceResult.Data;

            // 4. Create the main booking entity
            var booking = new Booking
            {
                UserId = appUser.User.UserId,
                FlightInstanceId = createDto.FlightInstanceId,
                BookingRef = Guid.NewGuid().ToString().Substring(0, 10).ToUpper(),
                BookingTime = DateTime.UtcNow,
                PriceTotal = totalPrice,
                PaymentStatus = BookingStatus.Pending.ToString(),
                FareBasisCodeId = createDto.FareBasisCode
            };

            await _unitOfWork.Bookings.AddAsync(booking);
            await _unitOfWork.SaveChangesAsync();

            // 5. Add passengers to the booking
            // Transform PassengerDetailsDto to CreatePassengerDto
            var createPassengerDtos = createDto.Passengers.Select(p => new CreatePassengerDto
            {
                FirstName = p.FirstName,
                LastName = p.LastName,
                DateOfBirth = p.DateOfBirth,
                PassportNumber = p.PassportNumber,
                UserId = appUser.User.UserId // Linking passenger to the user's UserId
            }).ToList();

            var addPassengersResult = await _passengerService.AddMultiplePassengersAsync(createPassengerDtos, booking.BookingId);
            if (!addPassengersResult.IsSuccess)
            {
                // Rollback the booking creation if passengers cannot be added
                _unitOfWork.Bookings.Remove(booking);
                await _unitOfWork.SaveChangesAsync();
                return ServiceResult<BookingDto>.Failure("Failed to add passengers to booking.");
            }

            // Map to DTO and return
            var bookingDto = new BookingDto
            {
                BookingId = booking.BookingId,
                BookingReference = booking.BookingRef,
                BookingTime = booking.BookingTime,
                TotalPrice = booking.PriceTotal ?? 0,
                PaymentStatus = booking.PaymentStatus,
                FlightNumber = flightInstance.Schedule.FlightNo,
                FareBasisCode = booking.FareBasisCodeId,
                Passengers = addPassengersResult.Data.ToList()
            };

            return ServiceResult<BookingDto>.Success(bookingDto);
        }

        public async Task<ServiceResult<BookingDto>> GetBookingByIdAsync(int bookingId)
        {
            var booking = await _unitOfWork.Bookings.GetWithDetailsAsync(bookingId);
            if (booking == null)
            {
                return ServiceResult<BookingDto>.Failure("Booking not found.");
            }

            var bookingDto = new BookingDto
            {
                BookingId = booking.BookingId,
                BookingReference = booking.BookingRef,
                BookingTime = booking.BookingTime,
                TotalPrice = booking.PriceTotal ?? 0,
                PaymentStatus = booking.PaymentStatus,
                FlightNumber = booking.FlightInstance.Schedule.FlightNo,
                FareBasisCode = booking.FareBasisCodeId,
                Passengers = booking.BookingPassengers.Select(bp => new PassengerDto
                {
                    PassengerId = bp.Passenger.PassengerId,
                    FirstName = bp.Passenger.FirstName,
                    LastName = bp.Passenger.LastName,
                    DateOfBirth = bp.Passenger.DateOfBirth,
                    PassportNumber = bp.Passenger.PassportNumber,
                    FrequentFlyerCardNumber = bp.Passenger.User?.FrequentFlyer?.CardNumber
                }).ToList()
            };

            return ServiceResult<BookingDto>.Success(bookingDto);
        }

        public async Task<ServiceResult<BookingConfirmationDto>> ConfirmBookingAsync(int bookingId)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId); // Using GetByIdAsync here
            if (booking == null)
            {
                return ServiceResult<BookingConfirmationDto>.Failure("Booking not found.");
            }

            if (booking.PaymentStatus != BookingStatus.Confirmed.ToString()) // Assuming BookingStatus enum or string "Confirmed"
            {
                return ServiceResult<BookingConfirmationDto>.Failure("Payment not confirmed for this booking.");
            }

            // Need to fetch tickets *with passenger details* to create the DTO
            var ticketsResult = await _unitOfWork.Tickets.GetByBookingWithDetailsAsync(bookingId); // Assumes this method exists in ITicketRepository
            if (!ticketsResult.Any())
            {
                // Maybe tickets haven't been generated yet? Or payment status is wrong?
                _logger.LogWarning("No tickets found for confirmed Booking ID {BookingId}. Ticket generation might have failed.", bookingId);
                // Depending on business logic, you might try generating tickets here or just return failure.
                return ServiceResult<BookingConfirmationDto>.Failure("No tickets found for this confirmed booking. Please contact support.");
            }

            var confirmationDto = new BookingConfirmationDto
            {
                BookingId = booking.BookingId,
                BookingReference = booking.BookingRef,
                ConfirmationMessage = "Your booking is confirmed. E-tickets have been generated.",
                 
                GeneratedTickets = ticketsResult.Select(t => new TicketSummaryDto
                {
                    TicketId = t.TicketId,
                    TicketCode = t.TicketCode, // Assuming TicketCode exists on Ticket entity
                    PassengerId = t.PassengerId,
                    // Assuming the GetByBookingWithDetailsAsync includes Passenger details
                    PassengerName = (t.Passenger != null) ? $"{t.Passenger.FirstName} {t.Passenger.LastName}" : "N/A"
                }).ToList()
                
            };

            return ServiceResult<BookingConfirmationDto>.Success(confirmationDto);
        }

        public async Task<ServiceResult> CancelBookingAsync(int bookingId, string userId)
        {
            var booking = await _unitOfWork.Bookings.GetWithDetailsAsync(bookingId);

            // Corrected user access to get AppUser.Id
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (booking == null || booking.User.AppUserId != user?.Id) // Corrected access to AppUserId
            {
                return ServiceResult.Failure("Booking not found or user not authorized to cancel it.");
            }

            if (booking.PaymentStatus == BookingStatus.Cancelled.ToString())
            {
                return ServiceResult.Failure("Booking is already cancelled.");
            }

            // Perform soft delete on the booking and related entities
            _unitOfWork.Bookings.SoftDelete(booking);
            foreach (var bp in booking.BookingPassengers)
            {
                _unitOfWork.BookingPassengers.SoftDelete(bp);
            }
            await _unitOfWork.SaveChangesAsync();

            return ServiceResult.Success();
        }

         
        public async Task<ServiceResult> UpdateBookingStatusAsync(int bookingId, string newStatus)
        {
            // 1. البحث عن الحجز
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);

            if (booking == null)
            {
                return ServiceResult.Failure($"Booking with ID {bookingId} not found.");
            }

            // 2. Check the status to avoid unnecessary updates: Success($"Booking {booking Id} is already in status: {newStatus}.");
            if (booking.PaymentStatus.Equals(newStatus, StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResult.Success();
            }

            // 3. Status update
            booking.PaymentStatus = newStatus;

            // 4. Save changes
            _unitOfWork.Bookings.Update(booking);  
            await _unitOfWork.SaveChangesAsync();

            // 5. Recording the process
            _logger.LogInformation($"Booking ID {bookingId} status successfully updated to {newStatus} via Payment Webhook.");

            return ServiceResult.Success();
        }

    }
}
 