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
    // Service implementation for managing e-tickets.
    public class TicketService : ITicketService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TicketService> _logger;  
        private readonly IMapper _mapper;              
        private readonly IUserRepository _userRepository; 

        public TicketService(IUnitOfWork unitOfWork, ILogger<TicketService> logger, IMapper mapper, IUserRepository userRepository)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        // Generates tickets for all passengers in a confirmed booking.
        public async Task<ServiceResult<List<TicketDto>>> GenerateTicketsForBookingAsync(int bookingId)
        {
            _logger.LogInformation("Attempting to generate tickets for Booking ID {BookingId}.", bookingId);
            var booking = await _unitOfWork.Bookings.GetWithDetailsAsync(bookingId); // Needs Passengers included
            if (booking == null || booking.BookingPassengers == null || !booking.BookingPassengers.Any())
            {
                _logger.LogWarning("GenerateTickets: Booking {BookingId} not found or has no passengers.", bookingId);
                return ServiceResult<List<TicketDto>>.Failure("Booking not found or has no passengers.");
            }
            if (booking.PaymentStatus?.ToUpperInvariant() != "CONFIRMED")
            {
                _logger.LogWarning("GenerateTickets: Payment not confirmed for Booking {BookingId}.", bookingId);
                return ServiceResult<List<TicketDto>>.Failure("Payment must be confirmed before generating tickets.");
            }

            var generatedTickets = new List<Ticket>();
            var errors = new List<string>();

            foreach (var bookingPassenger in booking.BookingPassengers)
            {
                try
                {
                    // Check if ticket already exists
                    var existingTicket = await _unitOfWork.Tickets.GetByBookingAndPassengerAsync(bookingId, bookingPassenger.PassengerId);
                    if (existingTicket != null)
                    {
                        _logger.LogInformation("Ticket already exists for Booking {BookingId}, Passenger {PassengerId}. Skipping generation.", bookingId, bookingPassenger.PassengerId);
                        continue; // Skip if already generated
                    }

                    var passenger = bookingPassenger.Passenger;
                    if (passenger == null)
                    {
                        _logger.LogError("Critical data inconsistency: BookingPassenger link exists but Passenger {PassengerId} is null for Booking {BookingId}.", bookingPassenger.PassengerId, bookingId);
                        errors.Add($"Passenger data missing for internal ID {bookingPassenger.PassengerId}.");
                        continue;
                    }

                    var seat = bookingPassenger.SeatAssignment; // May be null if no seat assigned yet

                    var ticket = new Ticket
                    {
                        TicketCode = GenerateTicketNumber(booking.BookingRef, passenger.PassengerId), // Generate unique code
                        IssueDate = DateTime.UtcNow,
                        Status = TicketStatus.Issued,
                        PassengerId = passenger.PassengerId,
                        BookingId = booking.BookingId,
                        FlightInstanceId = booking.FlightInstanceId,
                        SeatId = seat?.SeatId,
                        FrequentFlyerId = passenger.User?.FrequentFlyerId, // Requires User.FF include in GetWithDetailsAsync
                        IsDeleted = false
                    };
                    generatedTickets.Add(ticket);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error preparing ticket for Passenger ID {PassengerId} in Booking ID {BookingId}.", bookingPassenger.PassengerId, bookingId);
                    errors.Add($"Failed to prepare ticket for passenger {bookingPassenger.Passenger?.LastName}: {ex.Message}");
                }
            }

            if (errors.Any())
            {
                // Decide strategy: return partial success or full failure? Let's fail if any error occurs during prep.
                return ServiceResult<List<TicketDto>>.Failure(errors);
            }

            if (!generatedTickets.Any())
            {
                _logger.LogInformation("No new tickets needed for Booking {BookingId} (all might exist).", bookingId);
                // Fetch existing tickets to return
                var existingTicketEntities = await _unitOfWork.Tickets.GetByBookingWithDetailsAsync(bookingId);
                var existingDtos = _mapper.Map<List<TicketDto>>(existingTicketEntities);
                return ServiceResult<List<TicketDto>>.Success(existingDtos);
            }

            try
            {
                await _unitOfWork.Tickets.AddRangeAsync(generatedTickets);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully generated {Count} new tickets for Booking {BookingId}.", generatedTickets.Count, bookingId);

                // Map newly generated tickets to DTOs
                var generatedDtos = _mapper.Map<List<TicketDto>>(generatedTickets);
                // Enrich DTOs (could be done in mapper with more complex setup)
                foreach (var dto in generatedDtos)
                {
                    var bp = booking.BookingPassengers.FirstOrDefault(bpass => bpass.PassengerId == dto.PassengerId);
                    dto.PassengerName = bp?.Passenger != null ? $"{bp.Passenger.FirstName} {bp.Passenger.LastName}" : "N/A";
                    dto.FlightNumber = booking.FlightInstance?.Schedule?.FlightNo ?? "N/A";
                    dto.SeatNumber = bp?.SeatAssignment?.SeatNumber ?? "N/A";
                    dto.BookingReference = booking.BookingRef;
                    dto.FlightDepartureTime = booking.FlightInstance?.ScheduledDeparture ?? default;
                    dto.Status = TicketStatus.Issued.ToString();
                }

                 

                return ServiceResult<List<TicketDto>>.Success(generatedDtos); // Return only newly generated
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error saving generated tickets for Booking {BookingId}.", bookingId);
                return ServiceResult<List<TicketDto>>.Failure($"Failed to save generated tickets: {ex.Message}");
            }
        }

        // Retrieves a specific ticket by its ID with detailed information.
        public async Task<ServiceResult<TicketDetailDto>> GetTicketDetailsByIdAsync(int ticketId, ClaimsPrincipal user)
        {
            _logger.LogDebug("Retrieving details for Ticket ID {TicketId}.", ticketId);
            try
            {
                // Need a repo method that includes ALL related details
                var ticket = await _unitOfWork.Tickets.GetWithFullDetailsAsync(ticketId); // Assumes this method exists
                if (ticket == null) return ServiceResult<TicketDetailDto>.Failure("Ticket not found.");

                // Authorization: Check if user owns the booking or is admin
                var authResult = await AuthorizeTicketAccessAsync(user, ticket);
                if (!authResult.IsSuccess) return ServiceResult<TicketDetailDto>.Failure(authResult.Errors);

                var dto = _mapper.Map<TicketDetailDto>(ticket);
                return ServiceResult<TicketDetailDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving details for Ticket ID {TicketId}.", ticketId);
                return ServiceResult<TicketDetailDto>.Failure("An internal error occurred.");
            }
        }

        // Retrieves a specific ticket by its unique code with detailed information.
        public async Task<ServiceResult<TicketDetailDto>> GetTicketDetailsByCodeAsync(string ticketCode, ClaimsPrincipal user)
        {
            _logger.LogDebug("Retrieving details for Ticket Code {TicketCode}.", ticketCode);
            try
            {
                var ticket = await _unitOfWork.Tickets.GetByCodeWithDetailsAsync(ticketCode); // Includes details
                if (ticket == null) return ServiceResult<TicketDetailDto>.Failure("Ticket not found.");

                var authResult = await AuthorizeTicketAccessAsync(user, ticket);
                if (!authResult.IsSuccess) return ServiceResult<TicketDetailDto>.Failure(authResult.Errors);

                var dto = _mapper.Map<TicketDetailDto>(ticket);
                return ServiceResult<TicketDetailDto>.Success(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving details for Ticket Code {TicketCode}.", ticketCode);
                return ServiceResult<TicketDetailDto>.Failure("An internal error occurred.");
            }
        }

        // Retrieves all tickets associated with a specific booking.
        public async Task<ServiceResult<IEnumerable<TicketDto>>> GetTicketsByBookingAsync(int bookingId, ClaimsPrincipal user)
        {
            _logger.LogDebug("Retrieving tickets for Booking ID {BookingId}.", bookingId);
            try
            {
                var booking = await _unitOfWork.Bookings.GetActiveByIdAsync(bookingId);
                if (booking == null) return ServiceResult<IEnumerable<TicketDto>>.Failure("Booking not found.");
                var authResult = await AuthorizeBookingAccessAsync(user, booking); // Check access to booking first
                if (!authResult.IsSuccess) return ServiceResult<IEnumerable<TicketDto>>.Failure(authResult.Errors);

                var tickets = await _unitOfWork.Tickets.GetByBookingWithDetailsAsync(bookingId); // Includes Passenger
                var dtos = _mapper.Map<IEnumerable<TicketDto>>(tickets);
                // Enrich with data not easily mapped (can be improved with better mapping)
                foreach (var dto in dtos)
                {
                    dto.BookingReference = booking.BookingRef;
                    // Flight number and departure time might need fetching FlightInstance if not included
                }
                return ServiceResult<IEnumerable<TicketDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tickets for Booking ID {BookingId}.", bookingId);
                return ServiceResult<IEnumerable<TicketDto>>.Failure("An error occurred.");
            }
        }

        // Retrieves all tickets for the currently authenticated user (paginated).
        public async Task<ServiceResult<PaginatedResult<TicketDto>>> GetMyTicketsAsync(ClaimsPrincipal user, int pageNumber, int pageSize)
        {
            var appUserId = await _userRepository.GetUserIdFromClaimsPrincipalAsync(user);
            if (string.IsNullOrEmpty(appUserId)) return ServiceResult<PaginatedResult<TicketDto>>.Failure("Authentication required.");

            _logger.LogInformation("Retrieving tickets for User {UserId}, Page {PageNumber}.", appUserId, pageNumber);
            try
            {
                // Use the repository method directly
                var result = await _unitOfWork.Tickets.GetPaginatedByUserAsync(appUserId, pageNumber, pageSize); // Assumes this exists

                var dtos = _mapper.Map<List<TicketDto>>(result.Tickets); // Map the items

                var paginatedResult = new PaginatedResult<TicketDto>(dtos, result.TotalCount, pageNumber, pageSize);
                return ServiceResult<PaginatedResult<TicketDto>>.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tickets for User {UserId}.", appUserId);
                return ServiceResult<PaginatedResult<TicketDto>>.Failure("An error occurred.");
            }
        }

        // Updates the status of a specific ticket.
        public async Task<ServiceResult> UpdateTicketStatusAsync(int ticketId, UpdateTicketStatusDto statusDto, ClaimsPrincipal performingUser)
        {
            // Safely get the user name for logging. Use "SYSTEM" if the user is null (internal call).
            var userName = performingUser?.Identity?.Name ?? "SYSTEM";

            _logger.LogInformation("User {User} updating status for Ticket ID {TicketId} to {Status}.",
                userName, ticketId, statusDto.NewStatus);

            
            // Authorization: Allow internal system calls (null user) OR authorized staff
            if (performingUser != null) // Only check auth if a user principal is provided
            {
                var appUserId = await _userRepository.GetUserIdFromClaimsPrincipalAsync(performingUser);
                if (string.IsNullOrEmpty(appUserId))
                {
                    _logger.LogWarning("Authorization failed: Could not extract user ID from token.");
                    return ServiceResult.Failure("Access Denied.");
                }

                // Fetch the user from the database using their actual ID
                var performingAppUser = await _userRepository.GetByIdAsync(appUserId);

                // Authorization check: Must exist and must NOT be a regular 'User'
                if (performingAppUser == null || performingAppUser.UserType == UserType.User)
                {
                    _logger.LogWarning("Authorization failed: User {User} (ID: {UserId}) is null or does not have sufficient privileges (UserType: {UserType}) to update ticket status.",
                        performingUser?.Identity?.Name, appUserId, performingAppUser?.UserType.ToString() ?? "Unknown");
                    return ServiceResult.Failure("Access Denied.");
                }

            }
            try
            {
                var ticket = await _unitOfWork.Tickets.GetActiveByIdAsync(ticketId);
                if (ticket == null) return ServiceResult.Failure("Ticket not found.");

                // Basic validation (e.g., cannot revert from Boarded)
                if (ticket.Status == TicketStatus.Boarded && statusDto.NewStatus != TicketStatus.Boarded)
                    return ServiceResult.Failure("Cannot change status after passenger has boarded.");
                if (ticket.Status == TicketStatus.Cancelled)
                    return ServiceResult.Failure("Cannot change status of a cancelled ticket.");

                ticket.Status = statusDto.NewStatus;
                _unitOfWork.Tickets.Update(ticket);
                // TODO: Log status change with user and reason in an audit table.
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully updated status for Ticket ID {TicketId} to {Status}.", ticketId, statusDto.NewStatus);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for Ticket ID {TicketId}.", ticketId);
                return ServiceResult.Failure("An internal error occurred.");
            }
        }

        // Voids/Cancels a specific ticket.
        public async Task<ServiceResult> VoidTicketAsync(int ticketId, string reason, ClaimsPrincipal performingUser)
        {
            _logger.LogInformation("User {User} voiding Ticket ID {TicketId}. Reason: {Reason}", performingUser.Identity?.Name, ticketId, reason);
            // Use UpdateTicketStatusAsync for consistency
            var statusDto = new UpdateTicketStatusDto { NewStatus = TicketStatus.Cancelled, Reason = reason };
            // Authorization is handled within UpdateTicketStatusAsync
            return await UpdateTicketStatusAsync(ticketId, statusDto, performingUser);
        }

        // Performs a paginated search for tickets (Admin/Support).
        public async Task<ServiceResult<PaginatedResult<TicketDto>>> SearchTicketsAsync(TicketFilterDto filter, int pageNumber, int pageSize)
        {
            _logger.LogInformation("Admin searching tickets page {PageNumber}.", pageNumber);
            try
            {
                // Build filter expression (Ideally done in Repository)
                Expression<Func<Ticket, bool>> filterExpression = t => (filter.IncludeDeleted || !t.IsDeleted);
                if (!string.IsNullOrWhiteSpace(filter.TicketCode)) filterExpression = filterExpression.And(t => t.TicketCode == filter.TicketCode);
                if (filter.BookingId.HasValue) filterExpression = filterExpression.And(t => t.BookingId == filter.BookingId.Value);
                if (!string.IsNullOrWhiteSpace(filter.BookingReference)) filterExpression = filterExpression.And(t => t.Booking.BookingRef.Contains(filter.BookingReference)); // Needs Include
                if (filter.FlightInstanceId.HasValue) filterExpression = filterExpression.And(t => t.FlightInstanceId == filter.FlightInstanceId.Value);
                if (!string.IsNullOrWhiteSpace(filter.Status)) filterExpression = filterExpression.And(t => t.Status.ToString() == filter.Status);
                if (filter.IssueDateFrom.HasValue) filterExpression = filterExpression.And(t => t.IssueDate >= filter.IssueDateFrom.Value);
                if (filter.IssueDateTo.HasValue) filterExpression = filterExpression.And(t => t.IssueDate <= filter.IssueDateTo.Value);
                if (!string.IsNullOrWhiteSpace(filter.PassengerPassport)) filterExpression = filterExpression.And(t => t.Passenger.PassportNumber == filter.PassengerPassport); // Needs Include
                if (!string.IsNullOrWhiteSpace(filter.PassengerNameContains))
                {
                    var name = filter.PassengerNameContains.ToLower();
                    filterExpression = filterExpression.And(t => (t.Passenger.FirstName.ToLower() + " " + t.Passenger.LastName.ToLower()).Contains(name)); // Needs Include
                }


                // Get paged results - REQUIRES a GetPagedAsync in TicketRepository supporting Includes
                var (items, totalCount) = await _unitOfWork.Tickets.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    filterExpression,
                    orderBy: q => q.OrderByDescending(t => t.IssueDate),
                    includeProperties: "Booking,Passenger,FlightInstance.Schedule,Seat" // Specify needed includes
                );

                var dtos = _mapper.Map<List<TicketDto>>(items);
                // Enrich DTOs if needed (e.g., flight number, passenger name - better done in mapper)

                var paginatedResult = new PaginatedResult<TicketDto>(dtos, totalCount, pageNumber, pageSize);
                return ServiceResult<PaginatedResult<TicketDto>>.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching tickets.");
                return ServiceResult<PaginatedResult<TicketDto>>.Failure("An error occurred during ticket search.");
            }
        }
         
        // This is largely replaced by GetMyTicketsAsync
        public async Task<ServiceResult<IEnumerable<TicketDto>>> GetUserTicketsAsync(string userId)
        {
            _logger.LogWarning("Executing legacy GetUserTicketsAsync for AppUser ID {UserId}.", userId);
            // Assuming GetPaginatedByUserAsync exists and works correctly
            var ticketsResult = await _unitOfWork.Tickets.GetPaginatedByUserAsync(userId, 1, 1000); // Fetch up to 1000 tickets for compatibility
            if (ticketsResult == null || !ticketsResult.Tickets.Any())
            {
                return ServiceResult<IEnumerable<TicketDto>>.Failure("No tickets found for this user.");
            }
            var dtos = _mapper.Map<List<TicketDto>>(ticketsResult.Tickets);
            return ServiceResult<IEnumerable<TicketDto>>.Success(dtos);
        }

        // --- Helper Methods ---

        // Generates a unique ticket code/number.
        private string GenerateTicketNumber(string bookingRef, int passengerId)
        {
            // Example: Combine parts of booking ref, timestamp, passenger id hash
            // Ensure uniqueness check if necessary, although collision chance is low.
            var timestamp = DateTime.UtcNow.Ticks;
            return $"SQ{bookingRef?.Substring(0, 3)}{passengerId % 100}{timestamp % 10000}";
        }

        // Centralized authorization check for accessing ticket data.
        private async Task<ServiceResult> AuthorizeTicketAccessAsync(ClaimsPrincipal user, Ticket ticket)
        {
            var appUserId = await _userRepository.GetUserIdFromClaimsPrincipalAsync(user);
            if (string.IsNullOrEmpty(appUserId)) return ServiceResult.Failure("Authentication required.");

            // Check ownership via booking
            if (ticket.Booking?.User?.AppUserId == appUserId) // Need Booking.User.AppUserId included
            {
                return ServiceResult.Success(); // Owner access
            }

            // Check admin/operational roles
            if (user.IsInRole("Admin") || user.IsInRole("Supervisor") || user.IsInRole("SuperAdmin") || user.IsInRole("CheckInAgent") || user.IsInRole("GateAgent")) // Example roles
            {
                return ServiceResult.Success(); // Role-based access
            }

            _logger.LogWarning("User {UserId} unauthorized attempt to access Ticket ID {TicketId}.", appUserId, ticket.TicketId);
            return ServiceResult.Failure("Access denied to this ticket.");
        }

        // Duplicated authorization helper - Refactor needed.
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