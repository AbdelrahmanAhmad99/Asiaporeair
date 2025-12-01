using Application.DTOs.UserManagement;
using Application.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    // Interface for admin-facing management of all user accounts (CRUD, roles, status).
    public interface IUserManagementService
    {
        // Retrieves a paginated list of all users based on filters.
        Task<ServiceResult<PaginatedResult<UserSummaryDto>>> GetUsersPaginatedAsync(UserFilterDto filter, int pageNumber, int pageSize);

        // Retrieves the full profile details of any user by their ID.
        // This re-uses the detailed profile DTOs from AuthService.
        Task<ServiceResult<object>> GetUserDetailByIdAsync(string userId);

        // Deactivates (soft-deletes) a user account.
        Task<ServiceResult> DeactivateUserAsync(string userId, ClaimsPrincipal performingUser);

        // Reactivates a soft-deleted user account.
        Task<ServiceResult> ReactivateUserAsync(string userId, ClaimsPrincipal performingUser);

        // Updates the Identity roles for a specific user.
        Task<ServiceResult> UpdateUserRolesAsync(UpdateUserRolesDto dto, ClaimsPrincipal performingUser);

        // Triggers a password reset email to be sent to a user.
        Task<ServiceResult> TriggerPasswordResetAsync(string userId, ClaimsPrincipal performingUser);
    }
}