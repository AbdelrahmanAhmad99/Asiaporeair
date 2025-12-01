using Application.DTOs.Auth;
using Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Errors;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        #region --- Authentication Actions ---

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse>> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            var result = await _authService.LoginAsync(loginDto);

            if (!result.IsSuccess)
            {
              
                return Unauthorized(new ApiResponse(401, result.Errors.FirstOrDefault()));
            }

            return Ok(new ApiResponse(200, "Login successful", result.Data));
        }

        #endregion

        #region --- Password Management Endpoints ---

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse>> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            await _authService.ForgotPasswordAsync(forgotPasswordDto.Email);

            // We always achieve a successful response to prevent email detection
            return Ok(new ApiResponse(200, "If an account with this email exists, a password reset link has been sent."));
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse>> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            var result = await _authService.ResetPasswordAsync(resetPasswordDto);

            if (!result.IsSuccess)
            {
                return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
            }

            return Ok(new ApiResponse(200, "Your password has been reset successfully."));
        }

        [HttpPost("change-password")]
        [Authorize] // The user must be logged in
        public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            // `User` here is the ClaimsPrincipal for the current user
            var result = await _authService.ChangePasswordAsync(User, changePasswordDto);

            if (!result.IsSuccess)
            {
                return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
            }

            return Ok(new ApiResponse(200, "Your password has been changed successfully."));
        }

        #endregion

        #region --- Registration Actions ---

        [HttpPost("register-user")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse>> RegisterUser([FromForm] UserDto userDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            var result = await _authService.RegisterUserAsync(userDto);

            if (!result.IsSuccess)
            {
                 
                return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
            }

            return StatusCode(201, new ApiResponse(201, "User registered successfully"));
        }

        [HttpPost("register-superadmin")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse>> RegisterSuperAdmin([FromForm] SuperAdminDto superAdminDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            var result = await _authService.RegisterSuperAdminAsync(superAdminDto);

            if (!result.IsSuccess)
            {
                 
                return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
            }

            return StatusCode(201, new ApiResponse(201, "SuperAdmin registered successfully."));
        }

        [HttpPost("register-admin")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse>> RegisterAdmin([FromForm] AdminDto adminDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            var result = await _authService.RegisterAdminAsync(User, adminDto);

            if (!result.IsSuccess)
            {
                 
                return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
            }

            return StatusCode(201, new ApiResponse(201, "Admin registered successfully."));
        }

        [HttpPost("register-supervisor")]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<ActionResult<ApiResponse>> RegisterSupervisor([FromForm] SupervisorDto supervisorDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            var result = await _authService.RegisterSupervisorAsync(User, supervisorDto);

            if (!result.IsSuccess)
            {
                
                return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
            }

            return StatusCode(201, new ApiResponse(201, "Supervisor registered successfully."));
        }


        [HttpPost("register-pilot")]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<ActionResult<ApiResponse>> RegisterPilot([FromForm] PilotDto pilotDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            var result = await _authService.RegisterPilotAsync(User, pilotDto);

            if (!result.IsSuccess)
            {
                
                return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
            }

            return StatusCode(201, new ApiResponse(201, "Pilot registered successfully."));
        }

        [HttpPost("register-attendant")]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<ActionResult<ApiResponse>> RegisterAttendant([FromForm] AttendantDto attendantDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }

            var result = await _authService.RegisterAttendantAsync(User, attendantDto);

            if (!result.IsSuccess)
            {
                 
                return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
            }

            return StatusCode(201, new ApiResponse(201, "Attendant registered successfully."));
        }

        #endregion

        #region --- Profile Update Endpoints ---

        [HttpPut("profile/user")] // Use PUT for updates
        [Authorize(Roles = "User")] // Only authenticated Users (Passengers)
        public async Task<ActionResult<ApiResponse>> UpdateUserProfile([FromForm] UpdateUserProfileDto dto) // Use FromForm
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }
            // User updates their own profile
            var result = await _authService.UpdateUserProfileAsync(User, dto);
            if (!result.IsSuccess)
            {
                // Could be BadRequest (validation) or Forbidden (logic error)
                return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
            }
            return Ok(new ApiResponse(200, "Profile updated successfully."));
        }

        [HttpPut("profile/pilot/{pilotId}")] // Target specific pilot by ID
        [Authorize(Roles = "Pilot, Admin, SuperAdmin")] // Allowed roles
        public async Task<ActionResult<ApiResponse>> UpdatePilotProfile(string pilotId, [FromForm] UpdatePilotProfileDto dto) // Use FromForm
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }
            var result = await _authService.UpdatePilotProfileAsync(User, pilotId, dto);
            if (!result.IsSuccess)
            {
                // Check if the error is authorization-related
                if (result.Errors.Contains("Authorization denied."))
                    return Forbid(); // Return 403 Forbidden
                else
                    return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors }); // Other errors
            }
            return Ok(new ApiResponse(200, "Pilot profile updated successfully."));
        }

        [HttpPut("profile/admin/{adminId}")]
        [Authorize(Roles = "Admin, SuperAdmin")]
        public async Task<ActionResult<ApiResponse>> UpdateAdminProfile(string adminId, [FromForm] UpdateAdminProfileDto dto) // Use FromForm
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }
            var result = await _authService.UpdateAdminProfileAsync(User, adminId, dto);
            if (!result.IsSuccess)
            {
                if (result.Errors.Contains("Authorization denied.")) return Forbid();
                return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
            }
            return Ok(new ApiResponse(200, "Admin profile updated successfully."));
        }

        [HttpPut("profile/superadmin")] // SuperAdmin updates their own profile
        [Authorize(Roles = "SuperAdmin")]
        public async Task<ActionResult<ApiResponse>> UpdateSuperAdminProfile([FromForm] UpdateSuperAdminProfileDto dto) // Use FromForm
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }
            // Service method implicitly uses the current user's ID
            var result = await _authService.UpdateSuperAdminProfileAsync(User, dto);
            if (!result.IsSuccess)
            {
                // SuperAdmin update should generally only fail on validation or internal errors
                return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
            }
            return Ok(new ApiResponse(200, "Profile updated successfully."));
        }

        [HttpPut("profile/supervisor/{supervisorId}")]
        [Authorize(Roles = "Supervisor, Admin, SuperAdmin")]
        public async Task<ActionResult<ApiResponse>> UpdateSupervisorProfile(string supervisorId, [FromForm] UpdateSupervisorProfileDto dto) // Use FromForm
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }
            var result = await _authService.UpdateSupervisorProfileAsync(User, supervisorId, dto);
            if (!result.IsSuccess)
            {
                if (result.Errors.Contains("Authorization denied.")) return Forbid();
                return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
            }
            return Ok(new ApiResponse(200, "Supervisor profile updated successfully."));
        }

        [HttpPut("profile/attendant/{attendantId}")]
        [Authorize(Roles = "Attendant, Admin, SuperAdmin")]
        public async Task<ActionResult<ApiResponse>> UpdateAttendantProfile(string attendantId, [FromForm] UpdateAttendantProfileDto dto) // Use FromForm
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new ApiValidationErrorResponse { Errors = errors });
            }
            var result = await _authService.UpdateAttendantProfileAsync(User, attendantId, dto);
            if (!result.IsSuccess)
            {
                if (result.Errors.Contains("Authorization denied.")) return Forbid();
                return BadRequest(new ApiValidationErrorResponse { Errors = result.Errors });
            }
            return Ok(new ApiResponse(200, "Attendant profile updated successfully."));
        }

        #endregion
         
        #region --- Profile Retrieval Endpoints ---

        /// <summary>
        /// Gets the profile of the currently logged-in user.
        /// </summary>
        /// <returns>The user's profile DTO based on their role.</returns>
        [HttpGet("profile/me")]
        [Authorize] // Any authenticated user can get their own profile
        public async Task<ActionResult<ApiResponse>> GetMyProfile()
        {
            var result = await _authService.GetMyProfileAsync(User); // User is the ClaimsPrincipal

            if (!result.IsSuccess)
            {
                // Typically means user not found after authentication, which is unlikely but possible
                return NotFound(new ApiResponse(404, result.Errors.FirstOrDefault() ?? "Profile not found."));
            }

            // result.Data contains the specific DTO (e.g., PilotProfileDto, PassengerProfileDto)
            return Ok(new ApiResponse(200, "Profile retrieved successfully.", result.Data));
        }

        /// <summary>
        /// Gets the profile of a specific SuperAdmin by ID.
        /// </summary>
        /// <param name="id">The User ID (GUID) of the SuperAdmin.</param>
        /// <returns>The SuperAdmin's profile DTO.</returns>
        [HttpGet("profile/superadmin/{id}")]
        [Authorize(Roles = "SuperAdmin")] // Only SuperAdmins can view other SuperAdmins (or themselves)
        public async Task<ActionResult<ApiResponse>> GetSuperAdminProfile(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new ApiResponse(400, "User ID cannot be empty."));
            }

            var result = await _authService.GetSuperAdminByIdAsync(id);

            if (!result.IsSuccess)
            {
                return NotFound(new ApiResponse(404, result.Errors.FirstOrDefault() ?? $"SuperAdmin profile not found for ID: {id}"));
            }

            return Ok(new ApiResponse(200, "SuperAdmin profile retrieved successfully.", result.Data));
        }

        /// <summary>
        /// Gets the profile of a specific Admin by ID.
        /// </summary>
        /// <param name="id">The User ID (GUID) of the Admin.</param>
        /// <returns>The Admin's profile DTO.</returns>
        [HttpGet("profile/admin/{id}")]
        [Authorize(Roles = "Admin, SuperAdmin")] // Admins and SuperAdmins can view Admins
        public async Task<ActionResult<ApiResponse>> GetAdminProfile(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new ApiResponse(400, "User ID cannot be empty."));
            }

            var result = await _authService.GetAdminByIdAsync(id);

            if (!result.IsSuccess)
            {
                return NotFound(new ApiResponse(404, result.Errors.FirstOrDefault() ?? $"Admin profile not found for ID: {id}"));
            }

            return Ok(new ApiResponse(200, "Admin profile retrieved successfully.", result.Data));
        }

        /// <summary>
        /// Gets the profile of a specific User (Passenger) by ID.
        /// </summary>
        /// <param name="id">The User ID (GUID) of the Passenger.</param>
        /// <returns>The Passenger's profile DTO.</returns>
        [HttpGet("profile/user/{id}")]
        [Authorize(Roles = "Admin, SuperAdmin, User")] // Allow viewing by admins or the user themselves
        public async Task<ActionResult<ApiResponse>> GetUserProfile(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new ApiResponse(400, "User ID cannot be empty."));
            }

            // Authorization Check: Allow if user is getting their own profile OR is Admin/SuperAdmin
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id != currentUserId && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                return Forbid(); // 403 Forbidden if not authorized
            }

            var result = await _authService.GetUserProfileByIdAsync(id);

            if (!result.IsSuccess)
            {
                return NotFound(new ApiResponse(404, result.Errors.FirstOrDefault() ?? $"User profile not found for ID: {id}"));
            }

            return Ok(new ApiResponse(200, "User profile retrieved successfully.", result.Data));
        }

        /// <summary>
        /// Gets the profile of a specific Pilot by ID.
        /// </summary>
        /// <param name="id">The User ID (GUID) of the Pilot.</param>
        /// <returns>The Pilot's profile DTO.</returns>
        [HttpGet("profile/pilot/{id}")]
        [Authorize(Roles = "Pilot, Admin, SuperAdmin")] // Pilots, Admins, SuperAdmins can view Pilots
        public async Task<ActionResult<ApiResponse>> GetPilotProfile(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new ApiResponse(400, "User ID cannot be empty."));
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id != currentUserId && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                return Forbid();
            }

            var result = await _authService.GetPilotByIdAsync(id);

            if (!result.IsSuccess)
            {
                return NotFound(new ApiResponse(404, result.Errors.FirstOrDefault() ?? $"Pilot profile not found for ID: {id}"));
            }

            return Ok(new ApiResponse(200, "Pilot profile retrieved successfully.", result.Data));
        }

        /// <summary>
        /// Gets the profile of a specific Attendant by ID.
        /// </summary>
        /// <param name="id">The User ID (GUID) of the Attendant.</param>
        /// <returns>The Attendant's profile DTO.</returns>
        [HttpGet("profile/attendant/{id}")]
        [Authorize(Roles = "Attendant, Admin, SuperAdmin")] 
        public async Task<ActionResult<ApiResponse>> GetAttendantProfile(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new ApiResponse(400, "User ID cannot be empty."));
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // Allow viewing by self, Supervisor, Admin, or SuperAdmin
            if (id != currentUserId && !User.IsInRole("Supervisor") && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                // TODO: Add more specific check if Supervisor should only see attendants they manage
                return Forbid();
            }

            var result = await _authService.GetAttendantByIdAsync(id);

            if (!result.IsSuccess)
            {
                return NotFound(new ApiResponse(404, result.Errors.FirstOrDefault() ?? $"Attendant profile not found for ID: {id}"));
            }

            return Ok(new ApiResponse(200, "Attendant profile retrieved successfully.", result.Data));
        }

        /// <summary>
        /// Gets the profile of a specific Supervisor by ID.
        /// </summary>
        /// <param name="id">The User ID (GUID) of the Supervisor.</param>
        /// <returns>The Supervisor's profile DTO.</returns>
        [HttpGet("profile/supervisor/{id}")]
        [Authorize(Roles = "Supervisor, Admin, SuperAdmin")]
        public async Task<ActionResult<ApiResponse>> GetSupervisorProfile(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new ApiResponse(400, "User ID cannot be empty."));
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id != currentUserId && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
            {
                return Forbid();
            }

            var result = await _authService.GetSupervisorByIdAsync(id);

            if (!result.IsSuccess)
            {
                return NotFound(new ApiResponse(404, result.Errors.FirstOrDefault() ?? $"Supervisor profile not found for ID: {id}"));
            }

            return Ok(new ApiResponse(200, "Supervisor profile retrieved successfully.", result.Data));
        }

        #endregion


    }

    //  Dto for ForgotPassword
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}