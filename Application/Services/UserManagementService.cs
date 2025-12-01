using Application.DTOs.UserManagement;
using Application.Models;
using Application.Services.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Application.Services
{
    // Service implementation for admin-facing user account management.
    public class UserManagementService : IUserManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserManagementService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IAuthService _authService; // Dependency for re-using password reset

        public UserManagementService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<UserManagementService> logger,
            IUserRepository userRepository,
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IAuthService authService) // Inject IAuthService
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _userRepository = userRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _authService = authService;
        }

        // Retrieves a paginated list of all users based on filters.
        public async Task<ServiceResult<PaginatedResult<UserSummaryDto>>> GetUsersPaginatedAsync(UserFilterDto filter, int pageNumber, int pageSize)
        {
            _logger.LogInformation("Searching for users (Page: {PageNumber}) with filter: {FilterName}, {FilterEmail}, {FilterRole}, {IncludeDeleted}",
                pageNumber, filter.NameContains, filter.EmailContains, filter.UserType, filter.IncludeDeleted);
            try
            {
                // Build filter expression
                Expression<Func<AppUser, bool>> filterExpression = u => (filter.IncludeDeleted || !u.IsDeleted);

                if (filter.UserType.HasValue)
                    filterExpression = filterExpression.And(u => u.UserType == filter.UserType.Value);
                if (!string.IsNullOrWhiteSpace(filter.NameContains))
                    filterExpression = filterExpression.And(u => (u.FirstName + " " + u.LastName).Contains(filter.NameContains));
                if (!string.IsNullOrWhiteSpace(filter.EmailContains))
                    filterExpression = filterExpression.And(u => (u.Email != null && u.Email.Contains(filter.EmailContains)));

                // Get paged results
                // Using IUserRepository here would be better if it supported complex expressions.
                // Falling back to UserManager.Users
                var query = _userManager.Users.Where(filterExpression);

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderBy(u => u.LastName)
                    .ThenBy(u => u.FirstName)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var dtos = _mapper.Map<List<UserSummaryDto>>(items);
                var paginatedResult = new PaginatedResult<UserSummaryDto>(dtos, totalCount, pageNumber, pageSize);
                return ServiceResult<PaginatedResult<UserSummaryDto>>.Success(paginatedResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for users on page {PageNumber}.", pageNumber);
                return ServiceResult<PaginatedResult<UserSummaryDto>>.Failure("An error occurred during user search.");
            }
        }

        // Retrieves the full profile details of any user by their ID.
        public async Task<ServiceResult<object>> GetUserDetailByIdAsync(string userId)
        {
            _logger.LogInformation("Retrieving full user details for User ID {UserId}.", userId);
            var appUser = await _userRepository.GetByIdAsync(userId); // Use repo method, includes some details
            if (appUser == null) return ServiceResult<object>.Failure("User not found.");

            // Delegate to the AuthService's specific profile retrievers
            try
            {
                switch (appUser.UserType)
                {
                    case UserType.SuperAdmin:
                        var saResult = await _authService.GetSuperAdminByIdAsync(userId);
                        return saResult.IsSuccess ? ServiceResult<object>.Success(saResult.Data) : ServiceResult<object>.Failure(saResult.Errors);
                    case UserType.Admin:
                        var adResult = await _authService.GetAdminByIdAsync(userId);
                        return adResult.IsSuccess ? ServiceResult<object>.Success(adResult.Data) : ServiceResult<object>.Failure(adResult.Errors);
                    case UserType.Supervisor:
                        var suResult = await _authService.GetSupervisorByIdAsync(userId);
                        return suResult.IsSuccess ? ServiceResult<object>.Success(suResult.Data) : ServiceResult<object>.Failure(suResult.Errors);
                    case UserType.Pilot:
                        var piResult = await _authService.GetPilotByIdAsync(userId);
                        return piResult.IsSuccess ? ServiceResult<object>.Success(piResult.Data) : ServiceResult<object>.Failure(piResult.Errors);
                    case UserType.Attendant:
                        var atResult = await _authService.GetAttendantByIdAsync(userId);
                        return atResult.IsSuccess ? ServiceResult<object>.Success(atResult.Data) : ServiceResult<object>.Failure(atResult.Errors);
                    case UserType.User:
                        var paResult = await _authService.GetUserProfileByIdAsync(userId);
                        return paResult.IsSuccess ? ServiceResult<object>.Success(paResult.Data) : ServiceResult<object>.Failure(paResult.Errors);
                    default:
                        _logger.LogWarning("GetUserDetailByIdAsync: Unsupported UserType '{UserType}' for User ID {UserId}.", appUser.UserType, userId);
                        return ServiceResult<object>.Failure($"Unsupported user type: {appUser.UserType}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user details for {UserId} via AuthService.", userId);
                return ServiceResult<object>.Failure("An error occurred while retrieving profile details.");
            }
        }

        // Deactivates (soft-deletes) a user account.
        public async Task<ServiceResult> DeactivateUserAsync(string userId, ClaimsPrincipal performingUser)
        {
            _logger.LogInformation("User {PerformingUser} attempting to deactivate User ID {UserId}.", performingUser.Identity?.Name, userId);

            // Authorization: Only Admin or SuperAdmin
            if (!performingUser.IsInRole("Admin") && !performingUser.IsInRole("SuperAdmin"))
            {
                _logger.LogWarning("Authorization failed: User {User} cannot deactivate accounts.", performingUser.Identity?.Name);
                return ServiceResult.Failure("Access Denied.");
            }

            var userToDeactivate = await _userManager.FindByIdAsync(userId);
            if (userToDeactivate == null) return ServiceResult.Failure("User not found.");

            // Business Rule: Cannot deactivate a SuperAdmin
            if (userToDeactivate.UserType == UserType.SuperAdmin)
            {
                _logger.LogWarning("Deactivation failed: Cannot deactivate a SuperAdmin account (User ID {UserId}).", userId);
                return ServiceResult.Failure("SuperAdmin accounts cannot be deactivated.");
            }

            try
            {
                // Use the repository method for soft deletion
                var result = await _userRepository.SoftDeleteUserAsync(userId);
                if (!result)
                {
                    _logger.LogError("Repository SoftDeleteUserAsync returned false for User ID {UserId}.", userId);
                    return ServiceResult.Failure("Failed to update user status in database.");
                }

                // TODO: Add logic to cancel user's future bookings? Or just log?
                _logger.LogInformation("Successfully deactivated User ID {UserId}.", userId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating User ID {UserId}.", userId);
                return ServiceResult.Failure("An internal error occurred during deactivation.");
            }
        }

        // Reactivates a soft-deleted user account.
        public async Task<ServiceResult> ReactivateUserAsync(string userId, ClaimsPrincipal performingUser)
        {
            _logger.LogInformation("User {PerformingUser} attempting to reactivate User ID {UserId}.", performingUser.Identity?.Name, userId);

            // Authorization: Only Admin or SuperAdmin
            if (!performingUser.IsInRole("Admin") && !performingUser.IsInRole("SuperAdmin"))
            {
                _logger.LogWarning("Authorization failed: User {User} cannot reactivate accounts.", performingUser.Identity?.Name);
                return ServiceResult.Failure("Access Denied.");
            }

            try
            {
                // Use repository method to reactivate (includes finding deleted users)
                var result = await _userRepository.ReactivateUserAsync(userId);
                if (!result)
                {
                    _logger.LogError("Repository ReactivateUserAsync returned false for User ID {UserId}.", userId);
                    return ServiceResult.Failure("User not found or failed to update status.");
                }
                _logger.LogInformation("Successfully reactivated User ID {UserId}.", userId);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reactivating User ID {UserId}.", userId);
                return ServiceResult.Failure("An internal error occurred during reactivation.");
            }
        }

        // Updates the Identity roles for a specific user.
        public async Task<ServiceResult> UpdateUserRolesAsync(UpdateUserRolesDto dto, ClaimsPrincipal performingUser)
        {
            _logger.LogInformation("User {PerformingUser} attempting to update roles for User ID {UserId}.", performingUser.Identity?.Name, dto.UserId);
            // Authorization: SuperAdmin only
            if (!performingUser.IsInRole("SuperAdmin"))
            {
                _logger.LogWarning("Authorization failed: User {User} cannot update roles.", performingUser.Identity?.Name);
                return ServiceResult.Failure("Access Denied. Only SuperAdmins can manage roles.");
            }

            var userToUpdate = await _userManager.FindByIdAsync(dto.UserId);
            if (userToUpdate == null) return ServiceResult.Failure("User not found.");

            // Business Rule: Cannot change roles of SuperAdmin (or self, if desired)
            if (userToUpdate.UserType == UserType.SuperAdmin)
                return ServiceResult.Failure("Roles for SuperAdmins cannot be modified.");

            // Validate that all requested roles actually exist
            foreach (var roleName in dto.Roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                    return ServiceResult.Failure($"Role '{roleName}' does not exist.");
            }

            try
            {
                var currentRoles = await _userManager.GetRolesAsync(userToUpdate);
                var rolesToAdd = dto.Roles.Except(currentRoles).ToList();
                var rolesToRemove = currentRoles.Except(dto.Roles).ToList();

                IdentityResult addResult = IdentityResult.Success;
                IdentityResult removeResult = IdentityResult.Success;

                if (rolesToAdd.Any())
                    addResult = await _userManager.AddToRolesAsync(userToUpdate, rolesToAdd);
                if (rolesToRemove.Any())
                    removeResult = await _userManager.RemoveFromRolesAsync(userToUpdate, rolesToRemove);

                if (!addResult.Succeeded || !removeResult.Succeeded)
                {
                    var addErrors = addResult.Errors.Select(e => e.Description);
                    var removeErrors = removeResult.Errors.Select(e => e.Description);
                    _logger.LogWarning("Failed to update roles for User ID {UserId}. AddErrors: {AddErrors}, RemoveErrors: {RemoveErrors}",
                        dto.UserId, string.Join(", ", addErrors), string.Join(", ", removeErrors));
                    return ServiceResult.Failure(addErrors.Concat(removeErrors));
                }
                _logger.LogInformation("Successfully updated roles for User ID {UserId}. Added: {Added}. Removed: {Removed}.",
                    dto.UserId, string.Join(",", rolesToAdd), string.Join(",", rolesToRemove));
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating roles for User ID {UserId}.", dto.UserId);
                return ServiceResult.Failure("An internal error occurred while updating roles.");
            }
        }

        // Triggers a password reset email to be sent to a user.
        public async Task<ServiceResult> TriggerPasswordResetAsync(string userId, ClaimsPrincipal performingUser)
        {
            _logger.LogInformation("User {PerformingUser} triggering password reset for User ID {UserId}.", performingUser.Identity?.Name, userId);
            // Authorization: Admin/SuperAdmin
            if (!performingUser.IsInRole("Admin") && !performingUser.IsInRole("SuperAdmin"))
            {
                _logger.LogWarning("Authorization failed: User {User} cannot trigger password resets.", performingUser.Identity?.Name);
                return ServiceResult.Failure("Access Denied.");
            }

            var userToReset = await _userManager.FindByIdAsync(userId);
            if (userToReset?.Email == null)
            {
                _logger.LogWarning("Password reset trigger failed: User ID {UserId} not found or has no email.", userId);
                // Return success to prevent user enumeration
                return ServiceResult.Success();
            }

            // Delegate to AuthService to handle token generation and email sending
            try
            {
                var resetResult = await _authService.ForgotPasswordAsync(userToReset.Email);
                if (!resetResult.IsSuccess)
                {
                    // AuthService should have logged the error
                    return ServiceResult.Failure(resetResult.Errors);
                }
                _logger.LogInformation("Successfully triggered password reset for User ID {UserId} (Email: {Email}).", userId, userToReset.Email);
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error triggering password reset for User ID {UserId}.", userId);
                return ServiceResult.Failure("An internal error occurred.");
            }
        }
    }
}