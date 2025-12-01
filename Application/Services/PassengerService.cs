using Application.DTOs.Passenger;
using Application.Models;
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;  

namespace Application.Services
{
    // Service implementation for managing passenger data.
    public class PassengerService : IPassengerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PassengerService> _logger;
        private readonly IUserRepository _userRepository; // Needed to get User ID from ClaimsPrincipal

        public PassengerService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PassengerService> logger, IUserRepository userRepository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _userRepository = userRepository;
        }


        public async Task<ServiceResult<List<PassengerDto>>> AddMultiplePassengersAsync(List<CreatePassengerDto> passengersDto, int bookingId)
        {
            var booking = await _unitOfWork.Bookings.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return ServiceResult<List<PassengerDto>>.Failure("Booking not found.");
            }

            var passengers = new List<Passenger>();
            var bookingPassengers = new List<BookingPassenger>();

            foreach (var dto in passengersDto)
            {
                var passenger = new Passenger
                {
                    UserId = dto.UserId ?? booking.UserId, // Link to the user who made the booking or a specific user
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    DateOfBirth = dto.DateOfBirth,
                    PassportNumber = dto.PassportNumber
                };
                passengers.Add(passenger);

                // Create the many-to-many relationship entity
                var bp = new BookingPassenger
                {
                    BookingId = bookingId,
                    Passenger = passenger,
                    // Seat assignment will be done in a separate step
                    SeatAssignmentId = null
                };
                bookingPassengers.Add(bp);
            }

            await _unitOfWork.Passengers.AddRangeAsync(passengers);
            await _unitOfWork.BookingPassengers.AddRangeAsync(bookingPassengers);
            await _unitOfWork.SaveChangesAsync();

            var passengerDtos = passengers.Select(p => new PassengerDto
            {
                PassengerId = p.PassengerId,
                FirstName = p.FirstName,
                LastName = p.LastName,
                DateOfBirth = p.DateOfBirth,
                PassportNumber = p.PassportNumber,
            }).ToList();

            return ServiceResult<List<PassengerDto>>.Success(passengerDtos);
        }

        // Creates or finds existing passenger profiles and links them to a booking.
        // Associates passengers with the user making the booking unless specified otherwise.
        public async Task<ServiceResult<List<PassengerDto>>> CreateOrUpdatePassengersForBookingAsync(List<CreatePassengerDto> passengersDto, int bookingId, int bookingUserId)
        {
            _logger.LogInformation("Processing {Count} passengers for Booking ID {BookingId} by User ID {UserId}.", passengersDto.Count, bookingId, bookingUserId);

            var booking = await _unitOfWork.Bookings.GetActiveByIdAsync(bookingId);
            if (booking == null)
            {
                _logger.LogWarning("Booking ID {BookingId} not found.", bookingId);
                return ServiceResult<List<PassengerDto>>.Failure("Booking not found.");
            }
            // Ensure the booking belongs to the user, although this might be handled by BookingService caller
            if (booking.UserId != bookingUserId)
            {
                _logger.LogError("User Mismatch: Booking {BookingId} belongs to User {OwnerId}, but User {RequesterId} is adding passengers.", bookingId, booking.UserId, bookingUserId);
                // Decide on handling: fail or proceed? Let's proceed but log error.
            }

            var addedOrUpdatedPassengers = new List<Passenger>();
            var bookingPassengers = new List<BookingPassenger>();
            var errors = new List<string>();

            foreach (var dto in passengersDto)
            {
                try
                {
                    // Determine the User ID to link this passenger profile to. Default to booking user.
                    int targetUserId = dto.UserId ?? bookingUserId;

                    // Attempt to find an existing passenger for this user with the same passport
                    // This avoids creating duplicate passenger profiles for the same person under one user account.
                    var existingPassenger = (await _unitOfWork.Passengers.FindByPassportAsync(dto.PassportNumber))
                                            .FirstOrDefault(p => p.UserId == targetUserId && !p.IsDeleted);

                    Passenger passengerToAddOrLink;
                    if (existingPassenger != null)
                    {
                        _logger.LogDebug("Found existing Passenger ID {PassengerId} for User {UserId} with Passport {Passport}.", existingPassenger.PassengerId, targetUserId, dto.PassportNumber);
                        // Optional: Update existing passenger details if they differ? For now, just reuse.
                        // You could add logic here to update FirstName/LastName/DOB if needed.
                        passengerToAddOrLink = existingPassenger;
                    }
                    else
                    {
                        _logger.LogDebug("Creating new Passenger profile for User {UserId} with Passport {Passport}.", targetUserId, dto.PassportNumber);
                        // Create new passenger if not found
                        passengerToAddOrLink = _mapper.Map<Passenger>(dto);
                        passengerToAddOrLink.UserId = targetUserId; // Link to the correct User
                        await _unitOfWork.Passengers.AddAsync(passengerToAddOrLink);
                        // We need to save here to get the PassengerId if it's new, before creating BookingPassenger
                        await _unitOfWork.SaveChangesAsync();
                    }
                    addedOrUpdatedPassengers.Add(passengerToAddOrLink);

                    // Check if this passenger is already linked to this *specific* booking
                    if (!await _unitOfWork.BookingPassengers.ExistsAsync(bookingId, passengerToAddOrLink.PassengerId))
                    {
                        // Create the link to the booking
                        var bp = new BookingPassenger
                        {
                            BookingId = bookingId,
                            PassengerId = passengerToAddOrLink.PassengerId,
                            IsDeleted = false
                            // Seat assignment is handled separately by SeatService
                        };
                        bookingPassengers.Add(bp);
                    }
                    else
                    {
                        _logger.LogWarning("Passenger ID {PassengerId} is already linked to Booking ID {BookingId}.", passengerToAddOrLink.PassengerId, bookingId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing passenger {FirstName} {LastName} for Booking {BookingId}.", dto.FirstName, dto.LastName, bookingId);
                    errors.Add($"Failed to process passenger {dto.FirstName} {dto.LastName}: {ex.Message}");
                }
            }

            if (errors.Any())
            {
                // Decide on rollback strategy if needed. For now, return failure.
                return ServiceResult<List<PassengerDto>>.Failure(errors);
            }

            try
            {
                // Add the booking links if any were created
                if (bookingPassengers.Any())
                {
                    await _unitOfWork.BookingPassengers.AddRangeAsync(bookingPassengers);
                    await _unitOfWork.SaveChangesAsync(); // Save the links
                }

                _logger.LogInformation("Successfully processed {Count} passengers for Booking ID {BookingId}.", addedOrUpdatedPassengers.Count, bookingId);
                var resultDtos = _mapper.Map<List<PassengerDto>>(addedOrUpdatedPassengers);
                return ServiceResult<List<PassengerDto>>.Success(resultDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error saving BookingPassenger links for Booking ID {BookingId}.", bookingId);
                return ServiceResult<List<PassengerDto>>.Failure($"Failed to link passengers to booking: {ex.Message}");
            }
        }

        // Retrieves details for a specific passenger by their ID.
        public async Task<ServiceResult<PassengerDto>> GetPassengerByIdAsync(int passengerId, ClaimsPrincipal user)
        {
            _logger.LogDebug("Attempting to retrieve Passenger ID {PassengerId}.", passengerId);
            var passenger = await _unitOfWork.Passengers.GetWithDetailsAsync(passengerId);
            if (passenger == null)
            {
                return ServiceResult<PassengerDto>.Failure("Passenger not found.");
            }

            // Authorization: Check if the current user owns this passenger profile or is an Admin/SuperAdmin
            var currentAppUserId = await _userRepository.GetUserIdFromClaimsPrincipalAsync(user);
            var currentUserProfile = await _unitOfWork.Users.GetUserByAppUserIdAsync(currentAppUserId ?? ""); // Get the User profile linked to AppUser

            bool isOwner = (currentUserProfile != null && passenger.UserId == currentUserProfile.UserId);
            bool isAdmin = user.IsInRole("Admin") || user.IsInRole("SuperAdmin") || user.IsInRole("Supervisor");

            if (!isOwner && !isAdmin)
            {
                _logger.LogWarning("User {UserId} unauthorized attempt to access Passenger ID {PassengerId} owned by User ID {OwnerId}.", currentAppUserId, passengerId, passenger.UserId);
                return ServiceResult<PassengerDto>.Failure("Access denied.");
            }

            var dto = _mapper.Map<PassengerDto>(passenger);
            return ServiceResult<PassengerDto>.Success(dto);
        }

        // Updates details of an existing passenger profile.
        public async Task<ServiceResult<PassengerDto>> UpdatePassengerAsync(int passengerId, UpdatePassengerDto updateDto, ClaimsPrincipal user)
        {
            _logger.LogInformation("User {User} attempting to update Passenger ID {PassengerId}.", user.Identity?.Name, passengerId);
            var passenger = await _unitOfWork.Passengers.GetWithDetailsAsync(passengerId);
            if (passenger == null)
            {
                return ServiceResult<PassengerDto>.Failure("Passenger not found.");
            }

            // Authorization check (same as GetPassengerByIdAsync)
            var currentAppUserId = await _userRepository.GetUserIdFromClaimsPrincipalAsync(user);
            var currentUserProfile = await _unitOfWork.Users.GetUserByAppUserIdAsync(currentAppUserId ?? "");

            bool isOwner = (currentUserProfile != null && passenger.UserId == currentUserProfile.UserId);
            bool isAdmin = user.IsInRole("Admin") || user.IsInRole("SuperAdmin") || user.IsInRole("Supervisor");

            if (!isOwner && !isAdmin)
            {
                _logger.LogWarning("User {UserId} unauthorized attempt to update Passenger ID {PassengerId} owned by User ID {OwnerId}.", currentAppUserId, passengerId, passenger.UserId);
                return ServiceResult<PassengerDto>.Failure("Access denied.");
            }

            // Check if passport number is being changed and if it already exists for another passenger under this user
            if (!string.IsNullOrWhiteSpace(updateDto.PassportNumber) &&
                !passenger.PassportNumber.Equals(updateDto.PassportNumber, StringComparison.OrdinalIgnoreCase))
            {
                var existingPassport = (await _unitOfWork.Passengers.FindByPassportAsync(updateDto.PassportNumber))
                                       .FirstOrDefault(p => p.UserId == passenger.UserId && p.PassengerId != passengerId && !p.IsDeleted);
                if (existingPassport != null)
                {
                    _logger.LogWarning("Passport number {Passport} already exists for Passenger ID {ExistingId} under User ID {UserId}.", updateDto.PassportNumber, existingPassport.PassengerId, passenger.UserId);
                    return ServiceResult<PassengerDto>.Failure($"Passport number '{updateDto.PassportNumber}' is already associated with another passenger on this account.");
                }
            }

            try
            {
                // Apply updates using AutoMapper
                _mapper.Map(updateDto, passenger);
                _unitOfWork.Passengers.Update(passenger);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully updated Passenger ID {PassengerId}.", passengerId);
                var resultDto = _mapper.Map<PassengerDto>(passenger);
                return ServiceResult<PassengerDto>.Success(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating Passenger ID {PassengerId}.", passengerId);
                return ServiceResult<PassengerDto>.Failure($"An error occurred while updating passenger details: {ex.Message}");
            }
        }

        // Retrieves all passenger profiles associated with the currently logged-in user.
        public async Task<ServiceResult<IEnumerable<PassengerDto>>> GetMyPassengersAsync(ClaimsPrincipal user)
        {
            var currentAppUserId = await _userRepository.GetUserIdFromClaimsPrincipalAsync(user);
            if (string.IsNullOrEmpty(currentAppUserId))
            {
                return ServiceResult<IEnumerable<PassengerDto>>.Failure("Authentication required.");
            }
            var currentUserProfile = await _unitOfWork.Users.GetUserByAppUserIdAsync(currentAppUserId);
            if (currentUserProfile == null)
            {
                // This AppUser doesn't have a corresponding 'User' profile (might be employee only)
                _logger.LogInformation("AppUser {AppUserId} does not have a linked passenger 'User' profile.", currentAppUserId);
                return ServiceResult<IEnumerable<PassengerDto>>.Success(new List<PassengerDto>()); // Return empty list
            }

            return await GetPassengersByUserIdAsync(currentUserProfile.UserId);
        }

        // Retrieves all passenger profiles associated with a specific user ID (for admins).
        public async Task<ServiceResult<IEnumerable<PassengerDto>>> GetPassengersByUserIdAsync(int userId)
        {
            _logger.LogInformation("Retrieving passengers linked to User ID {UserId}.", userId);
            try
            {
                var passengers = await _unitOfWork.Passengers.GetByUserIdAsync(userId);
                var dtos = _mapper.Map<IEnumerable<PassengerDto>>(passengers);
                return ServiceResult<IEnumerable<PassengerDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving passengers for User ID {UserId}.", userId);
                return ServiceResult<IEnumerable<PassengerDto>>.Failure("An error occurred while retrieving passengers.");
            }
        }

        // Retrieves all passengers associated with a specific booking ID.
        public async Task<ServiceResult<IEnumerable<PassengerDto>>> GetPassengersByBookingAsync(int bookingId)
        {
            _logger.LogInformation("Retrieving passengers for Booking ID {BookingId}.", bookingId);
            try
            {
                var passengers = await _unitOfWork.Passengers.GetByBookingAsync(bookingId); // Repository handles joins
                var dtos = _mapper.Map<IEnumerable<PassengerDto>>(passengers);
                return ServiceResult<IEnumerable<PassengerDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving passengers for Booking ID {BookingId}.", bookingId);
                return ServiceResult<IEnumerable<PassengerDto>>.Failure("An error occurred while retrieving passengers for the booking.");
            }
        }

        // Performs a paginated search for passenger profiles (admin/support use).
        public async Task<ServiceResult<PaginatedResult<PassengerDto>>> SearchPassengersAsync(PassengerFilterDto filter, int pageNumber, int pageSize)
        {
            _logger.LogInformation("Searching passengers for page {PageNumber}.", pageNumber);
            try
            {
                // Build filter expression
                Expression<Func<Passenger, bool>> filterExpression = p => (filter.IncludeDeleted || !p.IsDeleted);

                if (!string.IsNullOrWhiteSpace(filter.NameContains))
                {
                    var name = filter.NameContains.ToLower();
                    filterExpression = filterExpression.And(p => p.FirstName.ToLower().Contains(name) || p.LastName.ToLower().Contains(name));
                }
                if (!string.IsNullOrWhiteSpace(filter.PassportNumber))
                {
                    // Ensure comparison is case-insensitive if necessary, though passport is often exact
                    filterExpression = filterExpression.And(p => p.PassportNumber == filter.PassportNumber);
                }
                if (filter.LinkedUserId.HasValue)
                {
                    filterExpression = filterExpression.And(p => p.UserId == filter.LinkedUserId.Value);
                }
 
                // We need User and FrequentFlyer to populate the DTO correctly.
                var includeProperties = "User.FrequentFlyer";
               

                var (items, totalCount) = await _unitOfWork.Passengers.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    filterExpression,
                    orderBy: q => q.OrderBy(p => p.LastName).ThenBy(p => p.FirstName),
                    includeProperties: includeProperties // Pass the includes to the repository
                );

                var dtos = _mapper.Map<List<PassengerDto>>(items);
                var paginatedResult = new PaginatedResult<PassengerDto>(dtos, totalCount, pageNumber, pageSize);
                return ServiceResult<PaginatedResult<PassengerDto>>.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching passengers.");
                return ServiceResult<PaginatedResult<PassengerDto>>.Failure("An error occurred during passenger search.");
            }
        }

        // Soft-deletes a passenger profile. Requires authorization and dependency checks.
        public async Task<ServiceResult> DeletePassengerAsync(int passengerId, ClaimsPrincipal user)
        {
            _logger.LogInformation("User {User} attempting to delete Passenger ID {PassengerId}.", user.Identity?.Name, passengerId);
            var passenger = await _unitOfWork.Passengers.GetActiveByIdAsync(passengerId);
            if (passenger == null)
            {
                return ServiceResult.Failure("Passenger not found.");
            }

            // Authorization check (same as Get/Update)
            var currentAppUserId = await _userRepository.GetUserIdFromClaimsPrincipalAsync(user);
            var currentUserProfile = await _unitOfWork.Users.GetUserByAppUserIdAsync(currentAppUserId ?? "");

            bool isOwner = (currentUserProfile != null && passenger.UserId == currentUserProfile.UserId);
            bool isAdmin = user.IsInRole("Admin") || user.IsInRole("SuperAdmin") || user.IsInRole("Supervisor");

            if (!isOwner && !isAdmin)
            {
                _logger.LogWarning("User {UserId} unauthorized attempt to delete Passenger ID {PassengerId} owned by User ID {OwnerId}.", currentAppUserId, passengerId, passenger.UserId);
                return ServiceResult.Failure("Access denied.");
            }

            // Dependency Check: Is this passenger on any *active* (not past, not cancelled) bookings?
            var activeBookings = await _unitOfWork.BookingPassengers.GetByPassengerAsync(passengerId);
            bool isOnActiveBooking = activeBookings.Any(bp =>
                bp.Booking != null && !bp.Booking.IsDeleted &&
                bp.Booking.FlightInstance != null && !bp.Booking.FlightInstance.IsDeleted &&
                bp.Booking.FlightInstance.Status != "Arrived" &&
                //bp.Booking.FlightInstance.Status != "Cancelled" &&
                bp.Booking.PaymentStatus != "Cancelled" &&
                bp.Booking.FlightInstance.ScheduledDeparture > DateTime.UtcNow // Check if flight is in the future
            );

            if (isOnActiveBooking)
            {
                _logger.LogWarning("Delete failed for Passenger ID {PassengerId}. Passenger is linked to active or future bookings.", passengerId);
                return ServiceResult.Failure("Cannot delete passenger profile. Passenger is associated with active or upcoming bookings.");
            }

            try
            {
                _unitOfWork.Passengers.SoftDelete(passenger);
                // Also soft-delete associated BookingPassenger links? Or let them stay as history?
                // Let's keep the links for historical data, just delete the passenger profile.
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully soft-deleted Passenger ID {PassengerId}.", passengerId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Passenger ID {PassengerId}.", passengerId);
                return ServiceResult.Failure($"An error occurred while deleting the passenger: {ex.Message}");
            }
        }



        public async Task<ServiceResult<PassengerDto>> UpdatePassengerAsync(string passengerId, UpdatePassengerDto updateDto)
        {
            // Assuming the `UpdatePassengerAsync` method in the service takes an `int` passengerId
            // since the database schema uses an INT for passenger_id.
            if (!int.TryParse(passengerId, out var id))
            {
                return ServiceResult<PassengerDto>.Failure("Invalid passenger ID format.");
            }

            var passenger = await _unitOfWork.Passengers.GetByIdAsync(id);
            if (passenger == null)
            {
                return ServiceResult<PassengerDto>.Failure("Passenger not found.");
            }

            passenger.FirstName = updateDto.FirstName;
            passenger.LastName = updateDto.LastName;
            if (updateDto.DateOfBirth.HasValue)
            {
                passenger.DateOfBirth = updateDto.DateOfBirth.Value;
            }
            if (!string.IsNullOrEmpty(updateDto.PassportNumber))
            {
                passenger.PassportNumber = updateDto.PassportNumber;
            }

            _unitOfWork.Passengers.Update(passenger);
            await _unitOfWork.SaveChangesAsync();

            var passengerDto = new PassengerDto
            {
                PassengerId = passenger.PassengerId,
                FirstName = passenger.FirstName,
                LastName = passenger.LastName,
                DateOfBirth = passenger.DateOfBirth,
                PassportNumber = passenger.PassportNumber
            };

            return ServiceResult<PassengerDto>.Success(passengerDto);
        }

    }
}
 
 