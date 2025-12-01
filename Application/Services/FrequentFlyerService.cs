using Application.DTOs.FrequentFlyer;
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

namespace Application.Services
{
    // Service implementation for managing Frequent Flyer accounts.
    public class FrequentFlyerService : IFrequentFlyerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<FrequentFlyerService> _logger;
        private readonly IUserRepository _userRepository; // To get User ID from Claims

        public FrequentFlyerService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<FrequentFlyerService> logger, IUserRepository userRepository)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _userRepository = userRepository;
        }



        // Creates a new Frequent Flyer account and links it to an existing User profile.
        public async Task<ServiceResult<FrequentFlyerDto>> CreateAccountAsync(CreateFrequentFlyerDto dto, ClaimsPrincipal performingUser)
        {
            _logger.LogInformation("User {User} attempting to create Frequent Flyer account for User ID {TargetUserId}.", performingUser.Identity?.Name, dto.UserId);

            // Authorization: Ensure only Admin/SuperAdmin can create for others, or user creates for self?
            // Let's assume only Admins create accounts via dashboard for now.
            if (!performingUser.IsInRole("Admin") && !performingUser.IsInRole("SuperAdmin"))
            {
                _logger.LogWarning("Authorization failed: User {User} cannot create Frequent Flyer accounts.", performingUser.Identity?.Name);
                return ServiceResult<FrequentFlyerDto>.Failure("Access Denied.");
            }

            // 1. Validate User exists and doesn't already have an FF account
            var userProfile = await _unitOfWork.Users.GetUserProfileByIdAsync(dto.UserId); // Use Generic GetById for User entity
            if (userProfile == null || userProfile.IsDeleted)
            {
                return ServiceResult<FrequentFlyerDto>.Failure($"User with ID {dto.UserId} not found or is inactive.");
            }
            if (userProfile.FrequentFlyerId.HasValue)
            {
                var existingFlyer = await _unitOfWork.FrequentFlyers.GetActiveByIdAsync(userProfile.FrequentFlyerId.Value);
                if (existingFlyer != null)
                    return ServiceResult<FrequentFlyerDto>.Failure($"User ID {dto.UserId} is already linked to Frequent Flyer account {existingFlyer.CardNumber}.");
            }

            // 2. Validate Card Number uniqueness
            if (await _unitOfWork.FrequentFlyers.ExistsByCardNumberAsync(dto.CardNumber))
            {
                return ServiceResult<FrequentFlyerDto>.Failure($"Frequent Flyer card number '{dto.CardNumber}' already exists.");
            }

            try
            {
                // 3. Create FrequentFlyer entity
                var newFlyer = _mapper.Map<FrequentFlyer>(dto);

                await _unitOfWork.FrequentFlyers.AddAsync(newFlyer);
                await _unitOfWork.SaveChangesAsync(); // Save to get the new FlyerId

                // 4. Link User to FrequentFlyer
                userProfile.FrequentFlyerId = newFlyer.FlyerId;
                _unitOfWork.Users.TrackPassengerProfileForUpdate(userProfile); // Assuming IUserRepository has Update or Generic Repo handles User entity
                await _unitOfWork.SaveChangesAsync(); // Save the link

                _logger.LogInformation("Successfully created Frequent Flyer ID {FlyerId} ({CardNumber}) and linked to User ID {UserId}.", newFlyer.FlyerId, newFlyer.CardNumber, dto.UserId);

                // Map result DTO including user details
                var resultDto = _mapper.Map<FrequentFlyerDto>(newFlyer);
                resultDto.LinkedUserId = userProfile.UserId;
                resultDto.LinkedUserName = $"{userProfile.AppUser?.FirstName} {userProfile.AppUser?.LastName}"; // Need to include AppUser when fetching User profile

                return ServiceResult<FrequentFlyerDto>.Success(resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Frequent Flyer account for User ID {UserId}.", dto.UserId);
                // Consider more specific rollback logic if needed
                return ServiceResult<FrequentFlyerDto>.Failure($"An error occurred: {ex.Message}");
            }
        }

        // Retrieves the Frequent Flyer account details for the currently logged-in user.
        public async Task<ServiceResult<FrequentFlyerDto>> GetMyAccountAsync(ClaimsPrincipal user)
        {
            var appUserId = await _userRepository.GetUserIdFromClaimsPrincipalAsync(user);
            if (string.IsNullOrEmpty(appUserId))
            {
                return ServiceResult<FrequentFlyerDto>.Failure("Authentication required.");
            }

            var userProfile = await _unitOfWork.Users.GetUserByAppUserIdAsync(appUserId); // Get the User profile
            if (userProfile?.FrequentFlyerId == null)
            {
                return ServiceResult<FrequentFlyerDto>.Failure("No Frequent Flyer account linked to this user.");
            }

            return await GetAccountByFlyerIdInternal(userProfile.FrequentFlyerId.Value, userProfile);
        }

        // Retrieves Frequent Flyer account details by User ID (admin access).
        public async Task<ServiceResult<FrequentFlyerDto>> GetAccountByUserIdAsync(int userId)
        {
            _logger.LogInformation("Retrieving Frequent Flyer account for User ID {UserId}.", userId);
            var userProfile = await _unitOfWork.Users.GetUserProfileByIdAsync(userId); // Use Generic GetById
            if (userProfile?.FrequentFlyerId == null)
            {
                return ServiceResult<FrequentFlyerDto>.Failure($"No Frequent Flyer account linked to User ID {userId}.");
            }
            return await GetAccountByFlyerIdInternal(userProfile.FrequentFlyerId.Value, userProfile);
        }

        // Retrieves Frequent Flyer account details by Card Number.
        public async Task<ServiceResult<FrequentFlyerDto>> GetAccountByCardNumberAsync(string cardNumber)
        {
            _logger.LogInformation("Retrieving Frequent Flyer account for Card Number {CardNumber}.", cardNumber);
            var flyer = await _unitOfWork.FrequentFlyers.GetByCardNumberAsync(cardNumber);
            if (flyer == null)
            {
                return ServiceResult<FrequentFlyerDto>.Failure($"Frequent Flyer account with card number '{cardNumber}' not found or is inactive.");
            }
            // Need to find the linked User profile
            var userProfile = await _unitOfWork.Users.GetByFrequentFlyerIdAsync(flyer.FlyerId); // Assumes this method exists in IUserRepository
            return await GetAccountByFlyerIdInternal(flyer.FlyerId, userProfile);
        }

        // Internal helper to get DTO by Flyer ID and optionally include User details
        private async Task<ServiceResult<FrequentFlyerDto>> GetAccountByFlyerIdInternal(int flyerId, User? userProfile = null)
        {
            var flyer = await _unitOfWork.FrequentFlyers.GetActiveByIdAsync(flyerId);
            if (flyer == null)
            {
                return ServiceResult<FrequentFlyerDto>.Failure($"Frequent Flyer account with ID {flyerId} not found or is inactive.");
            }

            if (userProfile == null)
            {
                userProfile = await _unitOfWork.Users.GetByFrequentFlyerIdAsync(flyerId); // Fetch if not provided
            }

            var dto = _mapper.Map<FrequentFlyerDto>(flyer);
            if (userProfile != null)
            {
                dto.LinkedUserId = userProfile.UserId;
                // Ensure AppUser is loaded to get name
                if (userProfile.AppUser == null) userProfile.AppUser = await _userRepository.GetByIdAsync(userProfile.AppUserId);
                dto.LinkedUserName = userProfile.AppUser != null ? $"{userProfile.AppUser.FirstName} {userProfile.AppUser.LastName}" : "N/A";
            }

            return ServiceResult<FrequentFlyerDto>.Success(dto);
        }

        // Calculates and adds points for a completed and confirmed booking.
        // Refined from existing code.
        public async Task<ServiceResult<int>> AddPointsForBookingAsync(int bookingId)
        {
            _logger.LogInformation("Attempting to add points for Booking ID {BookingId}.", bookingId);
            var booking = await _unitOfWork.Bookings.GetWithDetailsAsync(bookingId); // Get details needed for points calc

            if (booking == null)
                return ServiceResult<int>.Failure("Booking not found.");
            if (booking.PaymentStatus?.ToUpperInvariant() != "CONFIRMED") // Safer check
                return ServiceResult<int>.Failure("Booking payment is not confirmed.");
            if (booking.User?.FrequentFlyerId == null)
                return ServiceResult<int>.Failure("Booking user is not linked to a Frequent Flyer account.");

            if (booking.PointsAwarded)
            {
                _logger.LogWarning("Points already awarded for Booking ID {BookingId}. No action taken.", bookingId);
                return ServiceResult<int>.Failure("Points have already been awarded for this booking.");
            }

            // Basic Points Calculation Logic (replace with sophisticated rules)
            // Example: 1 point per $1, plus bonus for fare class, plus bonus for FF tier
            decimal basePoints = booking.PriceTotal ?? 0;
            decimal fareMultiplier = 1.0m;
            if (booking.FareBasisCodeId?.Contains("BUS") ?? false) fareMultiplier = 1.5m;
            if (booking.FareBasisCodeId?.Contains("FIR") ?? false) fareMultiplier = 2.0m;

            var flyer = await _unitOfWork.FrequentFlyers.GetActiveByIdAsync(booking.User.FrequentFlyerId.Value);
            if (flyer == null)
                return ServiceResult<int>.Failure("Frequent Flyer account not found.");

            decimal tierMultiplier = 1.0m;
            if (flyer.Level == "Silver") tierMultiplier = 1.1m;
            if (flyer.Level == "Gold") tierMultiplier = 1.25m;
            // ... add PPS etc.

            int pointsToAdd = (int)Math.Floor(basePoints * fareMultiplier * tierMultiplier);

            if (pointsToAdd <= 0)
            {
                _logger.LogInformation("No points calculated for Booking ID {BookingId}.", bookingId);
                return ServiceResult<int>.Success(0); // Success, but 0 points added
            }

            try
            {
                var newTotal = await _unitOfWork.FrequentFlyers.UpdatePointsAsync(flyer.FlyerId, pointsToAdd);
                if (!newTotal.HasValue)
                {
                    _logger.LogError("Failed to update points in repository for Flyer ID {FlyerId}.", flyer.FlyerId);
                    return ServiceResult<int>.Failure("Failed to update points balance.");
                }

                // Mark the booking as "PointsAwarded = true"
                booking.PointsAwarded = true;
                _unitOfWork.Bookings.Update(booking); // (Ensure your UoW tracks changes to Booking)

                await _unitOfWork.SaveChangesAsync(); // Commit the points update

                _logger.LogInformation("Added {Points} points to Flyer ID {FlyerId} for Booking ID {BookingId}. New balance: {NewTotal}", pointsToAdd, flyer.FlyerId, bookingId, newTotal.Value);
                return ServiceResult<int>.Success(pointsToAdd);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving points update for Flyer ID {FlyerId}, Booking ID {BookingId}.", flyer.FlyerId, bookingId);
                return ServiceResult<int>.Failure($"An error occurred while adding points: {ex.Message}");
            }
        }

        // Manually adds or subtracts points from an account (admin action).
        public async Task<ServiceResult<FrequentFlyerDto>> ManualAdjustPointsAsync(int flyerId, UpdatePointsDto dto, ClaimsPrincipal performingUser)
        {
            _logger.LogInformation("User {User} attempting manual points adjustment ({PointsDelta}) for Flyer ID {FlyerId}. Reason: {Reason}",
                performingUser.Identity?.Name, dto.PointsDelta, flyerId, dto.Reason);

            // Authorization: Admin/SuperAdmin only
            if (!performingUser.IsInRole("Admin") && !performingUser.IsInRole("SuperAdmin"))
            {
                _logger.LogWarning("Authorization failed: User {User} cannot manually adjust points.", performingUser.Identity?.Name);
                return ServiceResult<FrequentFlyerDto>.Failure("Access Denied."); // Changed to generic type
            }

            var flyer = await _unitOfWork.FrequentFlyers.GetActiveByIdAsync(flyerId);
            if (flyer == null)
            {
                return ServiceResult<FrequentFlyerDto>.Failure($"Frequent Flyer account ID {flyerId} not found."); // Changed to generic type
            }

            if ((flyer.AwardPoints ?? 0) + dto.PointsDelta < 0)
            {
                _logger.LogWarning("Manual adjustment for Flyer ID {FlyerId} results in negative balance.", flyerId);
                // Continue, but log it (as per original logic)
            }

            try
            {
                var newTotal = await _unitOfWork.FrequentFlyers.UpdatePointsAsync(flyerId, dto.PointsDelta);
                if (!newTotal.HasValue)
                {
                    _logger.LogError("Failed to update points in repository for Flyer ID {FlyerId}.", flyerId);
                    return ServiceResult<FrequentFlyerDto>.Failure("Failed to update points balance.");
                }

                // TODO: Log the manual adjustment detail (who, when, why, how much) in a separate audit table.
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully adjusted points for Flyer ID {FlyerId}. New balance: {NewTotal}", flyerId, newTotal.Value);
                 
                // Call the internal helper to get the DTO with all user details
                var updatedDtoResult = await GetAccountByFlyerIdInternal(flyerId);
                if (!updatedDtoResult.IsSuccess)
                {
                    // This shouldn't happen, but good to check
                    _logger.LogWarning("Points updated, but failed to retrieve updated DTO for Flyer ID {FlyerId}.", flyerId);
                    return ServiceResult<FrequentFlyerDto>.Failure(updatedDtoResult.Errors);
                }

                return ServiceResult<FrequentFlyerDto>.Success(updatedDtoResult.Data);
               
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving manual points adjustment for Flyer ID {FlyerId}.", flyerId);
                return ServiceResult<FrequentFlyerDto>.Failure($"An error occurred: {ex.Message}");
            }
        }

        // Updates the tier level of a Frequent Flyer account.
        public async Task<ServiceResult<FrequentFlyerDto>> UpdateLevelAsync(int flyerId, string newLevel, ClaimsPrincipal performingUser)
        {
            _logger.LogInformation("User {User} attempting to update level to '{NewLevel}' for Flyer ID {FlyerId}.", performingUser.Identity?.Name, newLevel, flyerId);

            // Authorization: Admin/SuperAdmin only
            if (!performingUser.IsInRole("Admin") && !performingUser.IsInRole("SuperAdmin"))
            {
                _logger.LogWarning("Authorization failed: User {User} cannot update FF level.", performingUser.Identity?.Name);
                return ServiceResult<FrequentFlyerDto>.Failure("Access Denied."); // Changed to generic type
            }

            var flyer = await _unitOfWork.FrequentFlyers.GetActiveByIdAsync(flyerId);
            if (flyer == null)
            {
                return ServiceResult<FrequentFlyerDto>.Failure($"Frequent Flyer account ID {flyerId} not found."); // Changed to generic type
            }
             
            // If no change is needed, still return the *current* full DTO as a successful response.
            if (flyer.Level == newLevel)
            {
                _logger.LogWarning("Level for Flyer ID {FlyerId} is already set to {NewLevel}. No update performed.", flyerId, newLevel);
                var currentDtoResult = await GetAccountByFlyerIdInternal(flyerId);
                if (!currentDtoResult.IsSuccess)
                {
                    return ServiceResult<FrequentFlyerDto>.Failure(currentDtoResult.Errors);
                }
                return ServiceResult<FrequentFlyerDto>.Success(currentDtoResult.Data);
            } 

            try
            {
                flyer.Level = newLevel;
                _unitOfWork.FrequentFlyers.Update(flyer);
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Successfully updated level for Flyer ID {FlyerId} to {NewLevel}.", flyerId, newLevel);
                 
                var updatedDtoResult = await GetAccountByFlyerIdInternal(flyerId);
                if (!updatedDtoResult.IsSuccess)
                {
                    _logger.LogWarning("Level updated, but failed to retrieve updated DTO for Flyer ID {FlyerId}.", flyerId);
                    return ServiceResult<FrequentFlyerDto>.Failure(updatedDtoResult.Errors);
                }

                return ServiceResult<FrequentFlyerDto>.Success(updatedDtoResult.Data);
           
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating level for Flyer ID {FlyerId}.", flyerId);
                return ServiceResult<FrequentFlyerDto>.Failure($"An error occurred: {ex.Message}"); // Changed to generic type
            }
        }

        // Performs a paginated search for Frequent Flyer accounts (admin use).
        public async Task<ServiceResult<PaginatedResult<FrequentFlyerDto>>> SearchAccountsAsync(FrequentFlyerFilterDto filter, int pageNumber, int pageSize)
        {
            _logger.LogInformation("Searching Frequent Flyer accounts page {PageNumber}.", pageNumber);
            try
            {
                // Build filter expression
                Expression<Func<FrequentFlyer, bool>> filterExpression = f => (filter.IncludeDeleted || !f.IsDeleted);

                if (!string.IsNullOrWhiteSpace(filter.CardNumberContains))
                {
                    filterExpression = filterExpression.And(f => f.CardNumber.Contains(filter.CardNumberContains));
                }
                if (!string.IsNullOrWhiteSpace(filter.Level))
                {
                    filterExpression = filterExpression.And(f => f.Level == filter.Level);
                }
                if (filter.MinPoints.HasValue)
                {
                    filterExpression = filterExpression.And(f => f.AwardPoints >= filter.MinPoints.Value);
                }
                if (filter.MaxPoints.HasValue)
                {
                    filterExpression = filterExpression.And(f => f.AwardPoints <= filter.MaxPoints.Value);
                }

                var (items, totalCount) = await _unitOfWork.FrequentFlyers.GetPagedAsync(
                   pageNumber,
                   pageSize,
                   filterExpression,
                   orderBy: q => q.OrderBy(f => f.CardNumber)
               // No Includes needed if User info fetched separately
               );

                // Map to DTOs and enrich with User info (N+1 potential, optimize if needed)
                var dtos = new List<FrequentFlyerDto>();
                foreach (var item in items)
                {
                    var userProfile = await _unitOfWork.Users.GetByFrequentFlyerIdAsync(item.FlyerId);
                    var dto = _mapper.Map<FrequentFlyerDto>(item);
                    if (userProfile != null)
                    {
                        dto.LinkedUserId = userProfile.UserId;
                        if (userProfile.AppUser == null) userProfile.AppUser = await _userRepository.GetByIdAsync(userProfile.AppUserId);
                        dto.LinkedUserName = userProfile.AppUser != null ? $"{userProfile.AppUser.FirstName} {userProfile.AppUser.LastName}" : "N/A";
                    }
                    dtos.Add(dto);
                }

                var paginatedResult = new PaginatedResult<FrequentFlyerDto>(dtos, totalCount, pageNumber, pageSize);
                return ServiceResult<PaginatedResult<FrequentFlyerDto>>.Success(paginatedResult);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching Frequent Flyer accounts.");
                return ServiceResult<PaginatedResult<FrequentFlyerDto>>.Failure("An error occurred during search.");
            }
        }

        // Soft-deletes a Frequent Flyer account (admin action).
        public async Task<ServiceResult> DeleteAccountAsync(int flyerId, ClaimsPrincipal performingUser)
        {
            _logger.LogInformation("User {User} attempting to delete Flyer ID {FlyerId}.", performingUser.Identity?.Name, flyerId);

            // Authorization: Admin/SuperAdmin only
            if (!performingUser.IsInRole("Admin") && !performingUser.IsInRole("SuperAdmin"))
            {
                _logger.LogWarning("Authorization failed: User {User} cannot delete FF accounts.", performingUser.Identity?.Name);
                return ServiceResult.Failure("Access Denied.");
            }

            var flyer = await _unitOfWork.FrequentFlyers.GetActiveByIdAsync(flyerId);
            if (flyer == null)
            {
                return ServiceResult.Failure($"Frequent Flyer account ID {flyerId} not found.");
            }

            try
            {
                // Unlink from User profile first
                var userProfile = await _unitOfWork.Users.GetByFrequentFlyerIdAsync(flyerId);
                if (userProfile != null)
                {
                    userProfile.FrequentFlyerId = null;
                    _unitOfWork.Users.TrackPassengerProfileForUpdate(userProfile);
                    // SaveChangesAsync will handle this along with the flyer deletion
                }

                _unitOfWork.FrequentFlyers.SoftDelete(flyer);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Successfully soft-deleted Flyer ID {FlyerId} and unlinked from User.", flyerId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting Flyer ID {FlyerId}.", flyerId);
                return ServiceResult.Failure($"An error occurred: {ex.Message}");
            }
        }

        
    }
}
  