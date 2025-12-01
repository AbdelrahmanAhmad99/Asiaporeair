using Application.DTOs.Auth;
using Application.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Application.Services.Interfaces
{
    public interface IAuthService
    {
        // --- Authentication ---
        Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto loginDto);
  
        // Registration methods
        Task<ServiceResult<IdentityResult>> RegisterSuperAdminAsync(SuperAdminDto superAdminDto);
        Task<ServiceResult<IdentityResult>> RegisterUserAsync(UserDto UserDto);
        Task<ServiceResult<IdentityResult>> RegisterPilotAsync(ClaimsPrincipal user, PilotDto pilotDto);
        Task<ServiceResult<IdentityResult>> RegisterAttendantAsync(ClaimsPrincipal user, AttendantDto attendantDto);
        Task<ServiceResult<IdentityResult>> RegisterAdminAsync(ClaimsPrincipal user, AdminDto adminDto);
        Task<ServiceResult<IdentityResult>> RegisterSupervisorAsync(ClaimsPrincipal user, SupervisorDto supervisorDto);

        // --- Password Management ---
        Task<ServiceResult> ForgotPasswordAsync(string email);
        Task<ServiceResult> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<ServiceResult> ChangePasswordAsync(ClaimsPrincipal user, ChangePasswordDto changePasswordDto);

        // --- Profile Update Methods ---
        Task<ServiceResult> UpdateUserProfileAsync(ClaimsPrincipal user, UpdateUserProfileDto dto); // Renamed
        Task<ServiceResult> UpdatePilotProfileAsync(ClaimsPrincipal user, string pilotId, UpdatePilotProfileDto dto);
        Task<ServiceResult> UpdateAdminProfileAsync(ClaimsPrincipal user, string adminId, UpdateAdminProfileDto dto);
        Task<ServiceResult> UpdateSuperAdminProfileAsync(ClaimsPrincipal user, UpdateSuperAdminProfileDto dto);
        Task<ServiceResult> UpdateSupervisorProfileAsync(ClaimsPrincipal user, string supervisorId, UpdateSupervisorProfileDto dto);
        Task<ServiceResult> UpdateAttendantProfileAsync(ClaimsPrincipal user, string attendantId, UpdateAttendantProfileDto dto);


        // --- Profile Retrieval Methods ---

        /// <summary>
        /// Retrieves the profile DTO for the currently authenticated user based on their role.
        /// </summary>
        /// <param name="user">The ClaimsPrincipal of the authenticated user.</param>
        /// <returns>ServiceResult containing the appropriate profile DTO (e.g., PilotProfileDto, PassengerProfileDto) or errors.</returns>
        Task<ServiceResult<object>> GetMyProfileAsync(ClaimsPrincipal user);

        /// <summary>
        /// Retrieves the profile DTO for a specific SuperAdmin by their ID.
        /// </summary>
        /// <param name="id">The User ID (GUID) of the SuperAdmin.</param>
        /// <returns>ServiceResult containing SuperAdminProfileDto or errors.</returns>
        Task<ServiceResult<SuperAdminProfileDto>> GetSuperAdminByIdAsync(string id);

        /// <summary>
        /// Retrieves the profile DTO for a specific Admin by their ID.
        /// </summary>
        /// <param name="id">The User ID (GUID) of the Admin.</param>
        /// <returns>ServiceResult containing AdminProfileDto or errors.</returns>
        Task<ServiceResult<AdminProfileDto>> GetAdminByIdAsync(string id);

        /// <summary>
        /// Retrieves the profile DTO for a specific User (Passenger) by their ID.
        /// </summary>
        /// <param name="id">The User ID (GUID) of the Passenger.</param>
        /// <returns>ServiceResult containing PassengerProfileDto or errors.</returns>
        Task<ServiceResult<PassengerProfileDto>> GetUserProfileByIdAsync(string id); // Renamed for clarity

        /// <summary>
        /// Retrieves the profile DTO for a specific Pilot by their ID.
        /// </summary>
        /// <param name="id">The User ID (GUID) of the Pilot.</param>
        /// <returns>ServiceResult containing PilotProfileDto or errors.</returns>
        Task<ServiceResult<PilotProfileDto>> GetPilotByIdAsync(string id);

        /// <summary>
        /// Retrieves the profile DTO for a specific Attendant by their ID.
        /// </summary>
        /// <param name="id">The User ID (GUID) of the Attendant.</param>
        /// <returns>ServiceResult containing AttendantProfileDto or errors.</returns>
        Task<ServiceResult<AttendantProfileDto>> GetAttendantByIdAsync(string id);

        /// <summary>
        /// Retrieves the profile DTO for a specific Supervisor by their ID.
        /// </summary>
        /// <param name="id">The User ID (GUID) of the Supervisor.</param>
        /// <returns>ServiceResult containing SupervisorProfileDto or errors.</returns>
        Task<ServiceResult<SupervisorProfileDto>> GetSupervisorByIdAsync(string id);
         



    }
}
