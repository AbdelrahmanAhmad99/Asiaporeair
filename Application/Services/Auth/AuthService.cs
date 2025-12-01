using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Auth;
using Application.Models;
using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
 

namespace Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly IEmailService _emailService;
        private readonly IFileService _fileService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserRepository _userRepository; 

        public AuthService(
            IUnitOfWork unitOfWork,
            IJwtService jwtService,
            IEmailService emailService,
            IFileService fileService,
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager,
            IUserRepository userRepository) 
        {
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _emailService = emailService;
            _fileService = fileService;
            _signInManager = signInManager;
            _userManager = userManager;
            _userRepository = userRepository; 
        }


        #region --- Authentication Actions ---


        public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto loginDto)
        {
            var user = await _unitOfWork.UserManager.FindByEmailAsync(loginDto.Email);
            if (user == null || user.IsDeleted)
            {
                return ServiceResult<AuthResponseDto>.Failure("Invalid email or password.");
            }

            var isPasswordCorrect = await _unitOfWork.UserManager.CheckPasswordAsync(user, loginDto.Password);
            if (!isPasswordCorrect)
            {
                return ServiceResult<AuthResponseDto>.Failure("Invalid email or password.");
            }


            user.LastLogin = System.DateTime.UtcNow;
            await _unitOfWork.UserManager.UpdateAsync(user);
            var token = await _jwtService.GenerateTokenAsync(user);

            var response = new AuthResponseDto
            {

                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserType = user.UserType.ToString(),
                Token = token
            };

            return ServiceResult<AuthResponseDto>.Success(response);
        }

        #endregion


        #region --- Password Management Endpoints ---

        public async Task<ServiceResult> ForgotPasswordAsync(string email)
        {
            var user = await _unitOfWork.UserManager.FindByEmailAsync(email);
            if (user == null)
            {
                return ServiceResult.Success();
            }

            var token = await _unitOfWork.UserManager.GeneratePasswordResetTokenAsync(user);
            await _emailService.SendPasswordResetEmailAsync(email, user.UserName ?? email, token);
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var user = await _unitOfWork.UserManager.FindByEmailAsync(resetPasswordDto.Email);
            if (user == null)
            {
                return ServiceResult.Failure("Password reset failed due to an invalid token or email.");
            }

            var resetResult = await _unitOfWork.UserManager.ResetPasswordAsync(
                user,
                resetPasswordDto.Token,
                resetPasswordDto.NewPassword);

            if (!resetResult.Succeeded)
            {
                var errors = resetResult.Errors.Select(e => e.Description);
                return ServiceResult.Failure(errors);
            }
            return ServiceResult.Success();
        }

        public async Task<ServiceResult> ChangePasswordAsync(ClaimsPrincipal userPrincipal, ChangePasswordDto changePasswordDto)
        {
            var userId = _unitOfWork.UserManager.GetUserId(userPrincipal);
            if (string.IsNullOrEmpty(userId))
            {
                return ServiceResult.Failure("User not found or not authenticated.");
            }

            var user = await _unitOfWork.UserManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ServiceResult.Failure("User not found.");
            }

            var changePasswordResult = await _unitOfWork.UserManager.ChangePasswordAsync(
                user,
                changePasswordDto.CurrentPassword,
                changePasswordDto.NewPassword);

            if (!changePasswordResult.Succeeded)
            {
                var errors = changePasswordResult.Errors.Select(e => e.Description);
                return ServiceResult.Failure(errors);
            }

            await _emailService.SendPasswordChangedNotificationAsync(user.Email, user.UserName ?? user.Email);
            return ServiceResult.Success();
        }

        #endregion


        #region--- Registration Methods --- 

        /// <summary>
        /// Registers a new Passenger (customer). This is a public-facing registration.
        /// It takes a password from the DTO and handles profile picture upload.
        /// </summary>
        public async Task<ServiceResult<IdentityResult>> RegisterUserAsync(UserDto userDto)
            {
                // 1. Validation: Check if email already exists
                var existingUser = await _unitOfWork.UserManager.FindByEmailAsync(userDto.Email);
                if (existingUser != null)
                {
                    return ServiceResult<IdentityResult>.Failure($"Email '{userDto.Email}' is already taken.");
                }

                string? profilePicPath = null;
                try
                {
                    // 2. File Handling: Save profile picture if provided
                    if (userDto.ProfilePicture != null)
                    {
                        profilePicPath = await _fileService.SaveFileAsync(userDto.ProfilePicture, "ProfilePictures");
                    }

                    // 3. Entity Creation: Map DTO to AppUser
                    var appUser = new AppUser
                    {
                        UserName = userDto.Email,
                        Email = userDto.Email,
                        FirstName = userDto.FirstName,
                        LastName = userDto.LastName,
                        PhoneNumber = userDto.PhoneNumber,
                        DateOfBirth = userDto.DateOfBirth,
                        Address = userDto.Address,
                        ProfilePictureUrl = profilePicPath, // Set the saved path
                        UserType = UserType.User,
                    };

                    // Link the specific 'User' (passenger) profile
                    appUser.User = new User
                    {
                        FrequentFlyerId = userDto.FrequentFlyerId
                    };

                    // 4. Database Operation: Create user and add to role (uses DTO password)
                    var result = await CreateUserWithRoleAsync(appUser, userDto.Password, "User");

                    // 5. Rollback: If user creation fails, delete the uploaded file
                    if (!result.Succeeded)
                    {
                        if (profilePicPath != null)
                        {
                            _fileService.DeleteFile(profilePicPath);
                        }
                        return ServiceResult<IdentityResult>.Failure(result.Errors.Select(e => e.Description));
                    }

                    // 6. Success
                    return ServiceResult<IdentityResult>.Success(result);
                }
                catch (Exception ex)
                {
                    // General error handling (e.g., file service fails)
                    if (profilePicPath != null)
                    {
                        _fileService.DeleteFile(profilePicPath);
                    }
                    return ServiceResult<IdentityResult>.Failure($"An error occurred: {ex.Message}");
                }
            }

        /// <summary>
        /// Registers the first SuperAdmin for the airline. 
        /// This method generates a random password and sends a welcome email.
        /// </summary>
        public async Task<ServiceResult<IdentityResult>> RegisterSuperAdminAsync(SuperAdminDto superAdminDto)
        {
            // 1. Validation: Check if email already exists
            var existingUser = await _unitOfWork.UserManager.FindByEmailAsync(superAdminDto.Email);
            if (existingUser != null)
            {
                return ServiceResult<IdentityResult>.Failure($"Email '{superAdminDto.Email}' is already taken.");
            }

            // 2. File Handling
            string? profilePicPath = null;
            if (superAdminDto.ProfilePicture != null)
            {
                profilePicPath = await _fileService.SaveFileAsync(superAdminDto.ProfilePicture, "ProfilePictures");
            }

            // 3. Entity Creation
            var appUser = new AppUser
            {
                UserName = superAdminDto.Email,
                Email = superAdminDto.Email,
                FirstName = superAdminDto.FirstName,  
                LastName = superAdminDto.LastName,    
                PhoneNumber = superAdminDto.PhoneNumber,
                ProfilePictureUrl = profilePicPath,
                DateOfBirth = superAdminDto.DateOfBirth,
                Address = superAdminDto.Address,
                UserType = UserType.SuperAdmin,
            };

            var employee = new Employee
            {
                DateOfHire = superAdminDto.DateOfHire,
                Salary = superAdminDto.Salary,
                AppUser = appUser // Link employee back to AppUser
            };

            appUser.SuperAdmin = new SuperAdmin { Employee = employee };

            // 4. Password & DB Operation
            // --- Use the password from the DTO instead of generating a random one ---
            var result = await CreateUserWithRoleAsync(appUser, superAdminDto.Password, "SuperAdmin");

            // 5. Rollback
            if (!result.Succeeded)
            {
                if (profilePicPath != null) _fileService.DeleteFile(profilePicPath);
                return ServiceResult<IdentityResult>.Failure(result.Errors.Select(e => e.Description));
            }

            
            return ServiceResult<IdentityResult>.Success(result);
        }
          

        /// <summary>
        /// Registers a new Admin. This must be done by an authenticated SuperAdmin.
        /// Generates a random password and sends a welcome email.
        /// </summary>
        public async Task<ServiceResult<IdentityResult>> RegisterAdminAsync(ClaimsPrincipal user, AdminDto adminDto)
            {
                // 1. Authorization: Check if the caller is a SuperAdmin
                var (authResult, superAdminId) = await AuthorizeCallerAsync(user, "SuperAdmin");
                if (!authResult.IsSuccess)
                {
                    return ServiceResult<IdentityResult>.Failure(authResult.Errors);
                }

                // 2. Validation: Check if email already exists
                var existingUser = await _unitOfWork.UserManager.FindByEmailAsync(adminDto.Email);
                if (existingUser != null)
                {
                    return ServiceResult<IdentityResult>.Failure($"Email '{adminDto.Email}' is already taken.");
                }

                // 3. File Handling
                string? profilePicPath = null;
                if (adminDto.ProfilePicture != null)
                {
                    profilePicPath = await _fileService.SaveFileAsync(adminDto.ProfilePicture, "ProfilePictures");
                }

                // 4. Entity Creation
                var appUser = new AppUser
                {
                    UserName = adminDto.Email,
                    Email = adminDto.Email,
                    FirstName = adminDto.FirstName,
                    LastName = adminDto.LastName,
                    PhoneNumber = adminDto.PhoneNumber,
                    ProfilePictureUrl = profilePicPath,
                    DateOfBirth = adminDto.DateOfBirth,
                    Address = adminDto.Address,
                    UserType = UserType.Admin,
                };

                var employee = new Employee
                {
                    DateOfHire = adminDto.DateOfHire,
                    Salary = adminDto.Salary,
                    AppUser = appUser // Link employee back to AppUser
                };

                // Link Admin profile and track who added them
                appUser.Admin = new Admin { Employee = employee, AddedById = superAdminId };

                // 5. Password & DB Operation 
                var password = GenerateRandomPassword();
                var result = await CreateUserWithRoleAsync(appUser, password , "Admin");

                // 6. Rollback
                if (!result.Succeeded)
                {
                    if (profilePicPath != null) _fileService.DeleteFile(profilePicPath);
                    return ServiceResult<IdentityResult>.Failure(result.Errors.Select(e => e.Description));
                }

                // 7. Post-Success: Send welcome email
                await _emailService.SendUserCredientialsEmailAsync(
                    appUser.Email,
                    appUser.Email,
                    password);

                return ServiceResult<IdentityResult>.Success(result);
            }

            /// <summary>
            /// Registers a new Pilot. This must be done by an authenticated SuperAdmin.
            /// Generates a random password and sends a welcome email.
            /// </summary>
            public async Task<ServiceResult<IdentityResult>> RegisterPilotAsync(ClaimsPrincipal user, PilotDto pilotDto)
            {
                // 1. Authorization: Check if the caller is a SuperAdmin
                var (authResult, superAdminId) = await AuthorizeCallerAsync(user, "SuperAdmin");
                if (!authResult.IsSuccess)
                {
                    return ServiceResult<IdentityResult>.Failure(authResult.Errors);
                }

                // 2. Validation: Check if email already exists
                var existingUser = await _unitOfWork.UserManager.FindByEmailAsync(pilotDto.Email);
                if (existingUser != null)
                {
                    return ServiceResult<IdentityResult>.Failure($"Email '{pilotDto.Email}' is already taken.");
                }

                // 3. File Handling
                string? profilePicPath = null;
                if (pilotDto.ProfilePicture != null)
                {
                    profilePicPath = await _fileService.SaveFileAsync(pilotDto.ProfilePicture, "ProfilePictures");
                }

                // 4. Entity Creation
                var appUser = new AppUser
                {
                    UserName = pilotDto.Email,
                    Email = pilotDto.Email,
                    FirstName = pilotDto.FirstName,
                    LastName = pilotDto.LastName,
                    PhoneNumber = pilotDto.PhoneNumber,
                    ProfilePictureUrl = profilePicPath,
                    DateOfBirth = pilotDto.DateOfBirth,
                    Address = pilotDto.Address,
                    UserType = UserType.Pilot,
                };

                var employee = new Employee
                {
                    DateOfHire = pilotDto.DateOfHire,
                    Salary = pilotDto.Salary,
                    AppUser = appUser // Link employee back to AppUser
                };

                var crewMember = new CrewMember
                {
                    Employee = employee,
                    CrewBaseAirportId = pilotDto.CrewBaseAirport,
                    Position = "Pilot"
                };

                appUser.Pilot = new Pilot
                {
                    CrewMember = crewMember,
                    LicenseNumber = pilotDto.LicenseNumber,
                    TotalFlightHours = pilotDto.TotalFlightHours,
                    AircraftTypeId = pilotDto.AircraftTypeId,
                    LastSimCheckDate = pilotDto.LastSimCheckDate,
                    AddedById = superAdminId // Track who added the pilot
                };

                // 5. Password & DB Operation
                var password = GenerateRandomPassword();
                var result = await CreateUserWithRoleAsync(appUser, password, "Pilot");

                // 6. Rollback
                if (!result.Succeeded)
                {
                    if (profilePicPath != null) _fileService.DeleteFile(profilePicPath);
                    return ServiceResult<IdentityResult>.Failure(result.Errors.Select(e => e.Description));
                }

                // 7. Post-Success: Send welcome email
                await _emailService.SendUserCredientialsEmailAsync(
                    appUser.Email,
                    appUser.Email,
                    password);

                return ServiceResult<IdentityResult>.Success(result);
            }

            /// <summary>
            /// Registers a new Attendant. This must be done by an authenticated SuperAdmin.
            /// Generates a random password and sends a welcome email.
            /// </summary>
            public async Task<ServiceResult<IdentityResult>> RegisterAttendantAsync(ClaimsPrincipal user, AttendantDto attendantDto)
            {
                // 1. Authorization: Check if the caller is a SuperAdmin
                var (authResult, superAdminId) = await AuthorizeCallerAsync(user, "SuperAdmin");
                if (!authResult.IsSuccess)
                {
                    return ServiceResult<IdentityResult>.Failure(authResult.Errors);
                }

                // 2. Validation: Check if email already exists
                var existingUser = await _unitOfWork.UserManager.FindByEmailAsync(attendantDto.Email);
                if (existingUser != null)
                {
                    return ServiceResult<IdentityResult>.Failure($"Email '{attendantDto.Email}' is already taken.");
                }

                // 3. File Handling
                string? profilePicPath = null;
                if (attendantDto.ProfilePicture != null)
                {
                    profilePicPath = await _fileService.SaveFileAsync(attendantDto.ProfilePicture, "ProfilePictures");
                }

                // 4. Entity Creation
                var appUser = new AppUser
                {
                    UserName = attendantDto.Email,
                    Email = attendantDto.Email,
                    FirstName = attendantDto.FirstName,
                    LastName = attendantDto.LastName,
                    PhoneNumber = attendantDto.PhoneNumber,
                    ProfilePictureUrl = profilePicPath,
                    DateOfBirth = attendantDto.DateOfBirth,
                    Address = attendantDto.Address,
                    UserType = UserType.Attendant,
                };

                var employee = new Employee
                {
                    DateOfHire = attendantDto.DateOfHire,
                    Salary = attendantDto.Salary,
                    AppUser = appUser // Link employee back to AppUser
                };

                var crewMember = new CrewMember
                {
                    Employee = employee,
                    CrewBaseAirportId = attendantDto.CrewBaseAirport,
                    Position = "Attendant"
                };

                appUser.Attendant = new Attendant
                {
                    CrewMember = crewMember,
                    AddedById = superAdminId // Track who added the attendant
                };

                // 5. Password & DB Operation
                var password = GenerateRandomPassword();
                var result = await CreateUserWithRoleAsync(appUser, password, "Attendant");

                // 6. Rollback
                if (!result.Succeeded)
                {
                    if (profilePicPath != null) _fileService.DeleteFile(profilePicPath);
                    return ServiceResult<IdentityResult>.Failure(result.Errors.Select(e => e.Description));
                }

                // 7. Post-Success: Send welcome email
                await _emailService.SendUserCredientialsEmailAsync(
                    appUser.Email,
                    appUser.Email,
                    password);

                return ServiceResult<IdentityResult>.Success(result);
            }
       
             /// <summary>
             /// Registers a new Supervisor. This must be done by an authenticated SuperAdmin.
             /// Generates a random password and sends a welcome email.
             /// </summary>
             public async Task<ServiceResult<IdentityResult>> RegisterSupervisorAsync(ClaimsPrincipal user, SupervisorDto supervisorDto)
             {
                 // 1. Authorization: Check if the caller is a SuperAdmin
                 var (authResult, superAdminId) = await AuthorizeCallerAsync(user, "SuperAdmin");
                 if (!authResult.IsSuccess)
                 {
                     return ServiceResult<IdentityResult>.Failure(authResult.Errors);
                 }

                 // 2. Validation: Check if email already exists
                 var existingUser = await _unitOfWork.UserManager.FindByEmailAsync(supervisorDto.Email);
                 if (existingUser != null)
                 {
                     return ServiceResult<IdentityResult>.Failure($"Email '{supervisorDto.Email}' is already taken.");
                 }

                 // 3. File Handling
                 string? profilePicPath = null;
                 if (supervisorDto.ProfilePicture != null)
                 {
                     profilePicPath = await _fileService.SaveFileAsync(supervisorDto.ProfilePicture, "ProfilePictures");
                 }

                 // 4. Entity Creation
                 var appUser = new AppUser
                 {
                     UserName = supervisorDto.Email,
                     Email = supervisorDto.Email,
                     FirstName = supervisorDto.FirstName,
                     LastName = supervisorDto.LastName,
                     PhoneNumber = supervisorDto.PhoneNumber,
                     ProfilePictureUrl = profilePicPath,
                     DateOfBirth = supervisorDto.DateOfBirth,
                     Address = supervisorDto.Address,
                     UserType = UserType.Supervisor,
                 };

                 var employee = new Employee
                 {
                     DateOfHire = supervisorDto.DateOfHire,
                     Salary = supervisorDto.Salary,
                     AppUser = appUser // Link employee back to AppUser
                 };

                 // Link Supervisor profile and track who added them
                 appUser.Supervisor = new Supervisor { Employee = employee, AddedById = superAdminId };

                 // 5. Password & DB Operation
                 var password = GenerateRandomPassword();
                 var result = await CreateUserWithRoleAsync(appUser, password, "Supervisor");

                 // 6. Rollback
                 if (!result.Succeeded)
                 {
                     if (profilePicPath != null) _fileService.DeleteFile(profilePicPath);
                     return ServiceResult<IdentityResult>.Failure(result.Errors.Select(e => e.Description));
                 }

                 // 7. Post-Success: Send welcome email
                 await _emailService.SendUserCredientialsEmailAsync(
                     appUser.Email,
                     appUser.Email,
                     password);

                 return ServiceResult<IdentityResult>.Success(result);
             }


        #endregion


        #region--- Profile Update Methods ---

        /// <summary>
        /// Updates the profile for the currently authenticated User (Passenger).
        /// </summary>
        public async Task<ServiceResult> UpdateUserProfileAsync(ClaimsPrincipal user, UpdateUserProfileDto dto)
        {
            var currentUserId = await _userRepository.GetUserIdFromClaimsPrincipalAsync(user);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return ServiceResult.Failure("Authentication required.");
            }

            // Retrieve AppUser and the specific User profile
            var appUser = await _userRepository.GetByIdAsync(currentUserId);
            var userProfile = await _userRepository.GetPassengerProfileByUserIdAsync(currentUserId); // Use the correct method name

            if (appUser == null || userProfile == null)
            {
                return ServiceResult.Failure("User profile not found.");
            }

            // Check if the user is actually a 'User' type
            if (appUser.UserType != UserType.User)
            {
                return ServiceResult.Failure("Action not allowed for this user type.");
            }

            // Update AppUser fields
            bool appUserChanged = UpdateAppUserFromDto(appUser, dto.FirstName, dto.LastName, dto.PhoneNumber, dto.DateOfBirth, dto.Address);

            // Update User (Passenger)-specific fields
            bool userProfileChanged = false;
            if (dto.KrisFlyerTier != null && userProfile.KrisFlyerTier != dto.KrisFlyerTier)
            {
                userProfile.KrisFlyerTier = dto.KrisFlyerTier;
                userProfileChanged = true;
            }

            // Handle Profile Picture
            var (picResult, oldPicPath, newPicPath) = await HandleProfilePictureUpdate(appUser, dto.ProfilePicture);
            if (!picResult.IsSuccess)
            {
                return picResult; // Return failure from picture handling
            }
            bool pictureChanged = newPicPath != null;

            // Only save if something actually changed
            if (!appUserChanged && !userProfileChanged && !pictureChanged)
            {
                return ServiceResult.Success(); // No changes needed
            }

            // Mark User profile for update if changed
            if (userProfileChanged)
            {
                _userRepository.TrackPassengerProfileForUpdate(userProfile);
            }

            // Save AppUser changes (UserManager handles tracking)
            var userUpdateResult = await _userManager.UpdateAsync(appUser);
            if (!userUpdateResult.Succeeded)
            {
                // Rollback: Delete new picture if save fails
                if (pictureChanged && newPicPath != null)
                {
                    _fileService.DeleteFile(newPicPath);
                    appUser.ProfilePictureUrl = oldPicPath; // Revert path in memory
                }
                return ServiceResult.Failure(userUpdateResult.Errors.Select(e => e.Description));
            }

            // If AppUser saved successfully, save other tracked changes (UserProfile)
            await _unitOfWork.SaveChangesAsync();

            // If only picture changed and AppUser save was successful, delete the old picture
            if (pictureChanged && !string.IsNullOrEmpty(oldPicPath))
            {
                _fileService.DeleteFile(oldPicPath);
            }

            return ServiceResult.Success();
        }


        /// <summary>
        /// Updates a Pilot's profile.
        /// Authorized for: Self, Admin, SuperAdmin.
        /// </summary>
        public async Task<ServiceResult> UpdatePilotProfileAsync(ClaimsPrincipal user, string pilotId, UpdatePilotProfileDto dto)
        {
            // 1. Get Pilot profile (includes AppUser, Employee, CrewMember)
            var pilotProfile = await _userRepository.GetPilotProfileByUserIdAsync(pilotId);
            if (pilotProfile == null || pilotProfile.AppUser == null || pilotProfile.CrewMember?.Employee == null)
            {
                return ServiceResult.Failure("Pilot profile or associated data not found.");
            }
            var appUserToUpdate = pilotProfile.AppUser;
            var employeeToUpdate = pilotProfile.CrewMember.Employee;
            var crewMemberToUpdate = pilotProfile.CrewMember;


            // 2. Authorize the action
            var authResult = await AuthorizeUpdateAsync(user, pilotId, UserType.Pilot);
            if (!authResult.IsSuccess)
            {
                return authResult;
            }

            // 3. Track changes
            bool appUserChanged = UpdateAppUserFromDto(appUserToUpdate, dto.FirstName, dto.LastName, dto.PhoneNumber, dto.DateOfBirth, dto.Address);
            bool employeeChanged = UpdateEmployeeFromDto(employeeToUpdate, dto.DateOfHire, dto.Salary);
            bool crewMemberChanged = UpdateCrewMemberFromDto(crewMemberToUpdate, dto.CrewBaseAirportId);
            bool pilotChanged = UpdatePilotSpecificFromDto(pilotProfile, dto.LicenseNumber, dto.TotalFlightHours, dto.LastSimCheckDate);

            // Handle Profile Picture
            var (picResult, oldPicPath, newPicPath) = await HandleProfilePictureUpdate(appUserToUpdate, dto.ProfilePicture);
            if (!picResult.IsSuccess) return picResult;
            bool pictureChanged = newPicPath != null;

            // Check if any actual change occurred
            if (!appUserChanged && !employeeChanged && !crewMemberChanged && !pilotChanged && !pictureChanged)
            {
                return ServiceResult.Success(); // No update needed
            }

            // Mark entities for update
            if (employeeChanged) _userRepository.TrackEmployeeForUpdate(employeeToUpdate);
            if (crewMemberChanged) _userRepository.TrackCrewMemberForUpdate(crewMemberToUpdate);
            if (pilotChanged) _userRepository.TrackPilotProfileForUpdate(pilotProfile);

            // Save AppUser first
            var userUpdateResult = await _userManager.UpdateAsync(appUserToUpdate);
            if (!userUpdateResult.Succeeded)
            {
                if (pictureChanged && newPicPath != null)
                {
                    _fileService.DeleteFile(newPicPath);
                    appUserToUpdate.ProfilePictureUrl = oldPicPath;
                }
                return ServiceResult.Failure(userUpdateResult.Errors.Select(e => e.Description));
            }

            // Save other related entities
            await _unitOfWork.SaveChangesAsync();

            // Clean up old picture if successful
            if (pictureChanged && !string.IsNullOrEmpty(oldPicPath))
            {
                _fileService.DeleteFile(oldPicPath);
            }

            return ServiceResult.Success();
        }

        /// <summary>
        /// Updates an Admin's profile.
        /// Authorized for: Self, SuperAdmin.
        /// </summary>
        public async Task<ServiceResult> UpdateAdminProfileAsync(ClaimsPrincipal user, string adminId, UpdateAdminProfileDto dto)
        {
            var adminProfile = await _userRepository.GetAdminProfileByUserIdAsync(adminId);
            if (adminProfile == null || adminProfile.AppUser == null || adminProfile.Employee == null)
            {
                return ServiceResult.Failure("Admin profile or associated data not found.");
            }
            var appUserToUpdate = adminProfile.AppUser;
            var employeeToUpdate = adminProfile.Employee;

            var authResult = await AuthorizeUpdateAsync(user, adminId, UserType.Admin);
            if (!authResult.IsSuccess) return authResult;

            bool appUserChanged = UpdateAppUserFromDto(appUserToUpdate, dto.FirstName, dto.LastName, dto.PhoneNumber, dto.DateOfBirth, dto.Address);
            bool employeeChanged = UpdateEmployeeFromDto(employeeToUpdate, dto.DateOfHire, dto.Salary);
            bool adminChanged = UpdateAdminSpecificFromDto(adminProfile, dto.Department); // Implement this helper

            var (picResult, oldPicPath, newPicPath) = await HandleProfilePictureUpdate(appUserToUpdate, dto.ProfilePicture);
            if (!picResult.IsSuccess) return picResult;
            bool pictureChanged = newPicPath != null;

            if (!appUserChanged && !employeeChanged && !adminChanged && !pictureChanged) return ServiceResult.Success();

            if (employeeChanged) _userRepository.TrackEmployeeForUpdate(employeeToUpdate);
            if (adminChanged) _userRepository.TrackAdminProfileForUpdate(adminProfile);

            var userUpdateResult = await _userManager.UpdateAsync(appUserToUpdate);
            if (!userUpdateResult.Succeeded)
            {
                if (pictureChanged && newPicPath != null)
                {
                    _fileService.DeleteFile(newPicPath);
                    appUserToUpdate.ProfilePictureUrl = oldPicPath;
                }
                return ServiceResult.Failure(userUpdateResult.Errors.Select(e => e.Description));
            }

            await _unitOfWork.SaveChangesAsync();
            if (pictureChanged && !string.IsNullOrEmpty(oldPicPath)) _fileService.DeleteFile(oldPicPath);

            return ServiceResult.Success();
        }

        /// <summary>
        /// Updates a SuperAdmin's profile.
        /// Authorized for: Self only.
        /// </summary>
        public async Task<ServiceResult> UpdateSuperAdminProfileAsync(ClaimsPrincipal user, UpdateSuperAdminProfileDto dto)
        {
            var currentUserId = await _userRepository.GetUserIdFromClaimsPrincipalAsync(user);
            if (string.IsNullOrEmpty(currentUserId)) return ServiceResult.Failure("Authentication required.");

            var superAdminProfile = await _userRepository.GetSuperAdminProfileByUserIdAsync(currentUserId);
            if (superAdminProfile == null || superAdminProfile.AppUser == null || superAdminProfile.Employee == null)
            {
                return ServiceResult.Failure("SuperAdmin profile or associated data not found.");
            }
            var appUserToUpdate = superAdminProfile.AppUser;
            var employeeToUpdate = superAdminProfile.Employee;

            // SuperAdmin can only update themselves
            bool appUserChanged = UpdateAppUserFromDto(appUserToUpdate, dto.FirstName, dto.LastName, dto.PhoneNumber, dto.DateOfBirth, dto.Address);
            bool employeeChanged = UpdateEmployeeFromDto(employeeToUpdate, dto.DateOfHire, dto.Salary);

            var (picResult, oldPicPath, newPicPath) = await HandleProfilePictureUpdate(appUserToUpdate, dto.ProfilePicture);
            if (!picResult.IsSuccess) return picResult;
            bool pictureChanged = newPicPath != null;

            if (!appUserChanged && !employeeChanged && !pictureChanged) return ServiceResult.Success();

            if (employeeChanged) _userRepository.TrackEmployeeForUpdate(employeeToUpdate);
            // No specific SuperAdmin fields to track currently
            // _userRepository.TrackSuperAdminProfileForUpdate(superAdminProfile);

            var userUpdateResult = await _userManager.UpdateAsync(appUserToUpdate);
            if (!userUpdateResult.Succeeded)
            {
                if (pictureChanged && newPicPath != null)
                {
                    _fileService.DeleteFile(newPicPath);
                    appUserToUpdate.ProfilePictureUrl = oldPicPath;
                }
                return ServiceResult.Failure(userUpdateResult.Errors.Select(e => e.Description));
            }

            await _unitOfWork.SaveChangesAsync();
            if (pictureChanged && !string.IsNullOrEmpty(oldPicPath)) _fileService.DeleteFile(oldPicPath);

            return ServiceResult.Success();
        }

        /// <summary>
        /// Updates a Supervisor's profile.
        /// Authorized for: Self, Admin, SuperAdmin.
        /// </summary>
        public async Task<ServiceResult> UpdateSupervisorProfileAsync(ClaimsPrincipal user, string supervisorId, UpdateSupervisorProfileDto dto)
        {
            var supervisorProfile = await _userRepository.GetSupervisorProfileByUserIdAsync(supervisorId);
            if (supervisorProfile == null || supervisorProfile.AppUser == null || supervisorProfile.Employee == null)
            {
                return ServiceResult.Failure("Supervisor profile or associated data not found.");
            }
            var appUserToUpdate = supervisorProfile.AppUser;
            var employeeToUpdate = supervisorProfile.Employee;

            var authResult = await AuthorizeUpdateAsync(user, supervisorId, UserType.Supervisor);
            if (!authResult.IsSuccess) return authResult;

            bool appUserChanged = UpdateAppUserFromDto(appUserToUpdate, dto.FirstName, dto.LastName, dto.PhoneNumber, dto.DateOfBirth, dto.Address);
            bool employeeChanged = UpdateEmployeeFromDto(employeeToUpdate, dto.DateOfHire, dto.Salary);
            bool supervisorChanged = UpdateSupervisorSpecificFromDto(supervisorProfile, dto.ManagedArea); // Implement helper

            var (picResult, oldPicPath, newPicPath) = await HandleProfilePictureUpdate(appUserToUpdate, dto.ProfilePicture);
            if (!picResult.IsSuccess) return picResult;
            bool pictureChanged = newPicPath != null;

            if (!appUserChanged && !employeeChanged && !supervisorChanged && !pictureChanged) return ServiceResult.Success();

            if (employeeChanged) _userRepository.TrackEmployeeForUpdate(employeeToUpdate);
            if (supervisorChanged) _userRepository.TrackSupervisorProfileForUpdate(supervisorProfile);

            var userUpdateResult = await _userManager.UpdateAsync(appUserToUpdate);
            if (!userUpdateResult.Succeeded)
            {
                if (pictureChanged && newPicPath != null)
                {
                    _fileService.DeleteFile(newPicPath);
                    appUserToUpdate.ProfilePictureUrl = oldPicPath;
                }
                return ServiceResult.Failure(userUpdateResult.Errors.Select(e => e.Description));
            }

            await _unitOfWork.SaveChangesAsync();
            if (pictureChanged && !string.IsNullOrEmpty(oldPicPath)) _fileService.DeleteFile(oldPicPath);

            return ServiceResult.Success();
        }

        /// <summary>
        /// Updates an Attendant's profile.
        /// Authorized for: Self, Admin, SuperAdmin.
        /// </summary>
        public async Task<ServiceResult> UpdateAttendantProfileAsync(ClaimsPrincipal user, string attendantId, UpdateAttendantProfileDto dto)
        {
            var attendantProfile = await _userRepository.GetAttendantProfileByUserIdAsync(attendantId);
            if (attendantProfile == null || attendantProfile.AppUser == null || attendantProfile.CrewMember?.Employee == null)
            {
                return ServiceResult.Failure("Attendant profile or associated data not found.");
            }
            var appUserToUpdate = attendantProfile.AppUser;
            var employeeToUpdate = attendantProfile.CrewMember.Employee;
            var crewMemberToUpdate = attendantProfile.CrewMember;

            var authResult = await AuthorizeUpdateAsync(user, attendantId, UserType.Attendant);
            if (!authResult.IsSuccess) return authResult;

            bool appUserChanged = UpdateAppUserFromDto(appUserToUpdate, dto.FirstName, dto.LastName, dto.PhoneNumber, dto.DateOfBirth, dto.Address);
            bool employeeChanged = UpdateEmployeeFromDto(employeeToUpdate, dto.DateOfHire, dto.Salary);
            bool crewMemberChanged = UpdateCrewMemberFromDto(crewMemberToUpdate, dto.CrewBaseAirportId);
            // Add specific attendant updates if needed
            // bool attendantChanged = UpdateAttendantSpecificFromDto(attendantProfile, ...);

            var (picResult, oldPicPath, newPicPath) = await HandleProfilePictureUpdate(appUserToUpdate, dto.ProfilePicture);
            if (!picResult.IsSuccess) return picResult;
            bool pictureChanged = newPicPath != null;

            if (!appUserChanged && !employeeChanged && !crewMemberChanged && !pictureChanged  ) return ServiceResult.Success();

            if (employeeChanged) _userRepository.TrackEmployeeForUpdate(employeeToUpdate);
            if (crewMemberChanged) _userRepository.TrackCrewMemberForUpdate(crewMemberToUpdate);
            // if (attendantChanged) _userRepository.TrackAttendantProfileForUpdate(attendantProfile);

            var userUpdateResult = await _userManager.UpdateAsync(appUserToUpdate);
            if (!userUpdateResult.Succeeded)
            {
                if (pictureChanged && newPicPath != null)
                {
                    _fileService.DeleteFile(newPicPath);
                    appUserToUpdate.ProfilePictureUrl = oldPicPath;
                }
                return ServiceResult.Failure(userUpdateResult.Errors.Select(e => e.Description));
            }

            await _unitOfWork.SaveChangesAsync();
            if (pictureChanged && !string.IsNullOrEmpty(oldPicPath)) _fileService.DeleteFile(oldPicPath);

            return ServiceResult.Success();
        }

        #endregion


        #region --- Profile Retrieval Methods ---

        /// <summary>
        /// Gets the profile DTO for the currently authenticated user.
        /// </summary>
        public async Task<ServiceResult<object>> GetMyProfileAsync(ClaimsPrincipal user)
        {
            var userId = await _userRepository.GetUserIdFromClaimsPrincipalAsync(user);
            if (string.IsNullOrEmpty(userId))
            {
                return ServiceResult<object>.Failure("Authentication required. Unable to identify current user.");
            }

            // Fetch the user to determine their type
            var appUser = await _userRepository.GetByIdAsync(userId); // Use simple GetById here
            if (appUser == null)
            {
                return ServiceResult<object>.Failure("User profile not found.");
            }

            // Delegate to the specific method based on UserType
            switch (appUser.UserType)
            {
                case UserType.SuperAdmin:
                    var saResult = await GetSuperAdminByIdAsync(userId);
                    return saResult.IsSuccess
                        ? ServiceResult<object>.Success(saResult.Data)
                        : ServiceResult<object>.Failure(saResult.Errors);

                case UserType.Admin:
                    var adResult = await GetAdminByIdAsync(userId);
                    return adResult.IsSuccess
                        ? ServiceResult<object>.Success(adResult.Data)
                        : ServiceResult<object>.Failure(adResult.Errors);

                case UserType.Supervisor:
                    var suResult = await GetSupervisorByIdAsync(userId);
                    return suResult.IsSuccess
                        ? ServiceResult<object>.Success(suResult.Data)
                        : ServiceResult<object>.Failure(suResult.Errors);

                case UserType.Pilot:
                    var piResult = await GetPilotByIdAsync(userId);
                    return piResult.IsSuccess
                        ? ServiceResult<object>.Success(piResult.Data)
                        : ServiceResult<object>.Failure(piResult.Errors);

                case UserType.Attendant:
                    var atResult = await GetAttendantByIdAsync(userId);
                    return atResult.IsSuccess
                        ? ServiceResult<object>.Success(atResult.Data)
                        : ServiceResult<object>.Failure(atResult.Errors);

                case UserType.User: // Passenger
                    var paResult = await GetUserProfileByIdAsync(userId);
                    return paResult.IsSuccess
                        ? ServiceResult<object>.Success(paResult.Data)
                        : ServiceResult<object>.Failure(paResult.Errors);

                default:
                    // Fallback or handle unknown types
                    return ServiceResult<object>.Failure($"Unsupported user type: {appUser.UserType}");
            }
        }

        /// <summary>
        /// Gets the profile DTO for a specific SuperAdmin.
        /// </summary>
        public async Task<ServiceResult<SuperAdminProfileDto>> GetSuperAdminByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return ServiceResult<SuperAdminProfileDto>.Failure("User ID cannot be empty.");

            var superAdminProfile = await _userRepository.GetSuperAdminProfileByUserIdAsync(id);
            if (superAdminProfile?.AppUser == null || superAdminProfile.Employee == null)
            {
                return ServiceResult<SuperAdminProfileDto>.Failure($"SuperAdmin profile not found for ID: {id}");
            }

            var appUser = superAdminProfile.AppUser;
            var employee = superAdminProfile.Employee;

            var dto = new SuperAdminProfileDto
            {
                Id = appUser.Id,
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                Email = appUser.Email ?? string.Empty,
                UserName = appUser.UserName ?? string.Empty,
                PhoneNumber = appUser.PhoneNumber,
                ProfilePictureUrl = appUser.ProfilePictureUrl,
                DateOfBirth = appUser.DateOfBirth,
                Address = appUser.Address,
                DateCreated = appUser.DateCreated,
                LastLogin = appUser.LastLogin,
                UserType = appUser.UserType,
                EmployeeId = employee.EmployeeId,
                DateOfHire = employee.DateOfHire,
                Salary = employee.Salary // Consider security implications
            };

            return ServiceResult<SuperAdminProfileDto>.Success(dto);
        }

        /// <summary>
        /// Gets the profile DTO for a specific Admin.
        /// </summary>
        public async Task<ServiceResult<AdminProfileDto>> GetAdminByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return ServiceResult<AdminProfileDto>.Failure("User ID cannot be empty.");

            var adminProfile = await _userRepository.GetAdminProfileByUserIdAsync(id);
            if (adminProfile?.AppUser == null || adminProfile.Employee == null)
            {
                return ServiceResult<AdminProfileDto>.Failure($"Admin profile not found for ID: {id}");
            }

            var appUser = adminProfile.AppUser;
            var employee = adminProfile.Employee;

            var dto = new AdminProfileDto
            {
                Id = appUser.Id,
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                Email = appUser.Email ?? string.Empty,
                UserName = appUser.UserName ?? string.Empty,
                PhoneNumber = appUser.PhoneNumber,
                ProfilePictureUrl = appUser.ProfilePictureUrl,
                DateOfBirth = appUser.DateOfBirth,
                Address = appUser.Address,
                DateCreated = appUser.DateCreated,
                LastLogin = appUser.LastLogin,
                UserType = appUser.UserType,
                EmployeeId = employee.EmployeeId,
                DateOfHire = employee.DateOfHire,
                Salary = employee.Salary, // Consider security
                Department = adminProfile.Department, // Assuming you added this
                AddedById = adminProfile.AddedById,
                AddedByName = adminProfile.AddedBy != null ? $"{adminProfile.AddedBy.FirstName} {adminProfile.AddedBy.LastName}" : "N/A"
            };

            return ServiceResult<AdminProfileDto>.Success(dto);
        }

        /// <summary>
        /// Gets the profile DTO for a specific User (Passenger).
        /// </summary>
        public async Task<ServiceResult<PassengerProfileDto>> GetUserProfileByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return ServiceResult<PassengerProfileDto>.Failure("User ID cannot be empty.");

            var passengerProfile = await _userRepository.GetPassengerProfileByUserIdAsync(id);
            if (passengerProfile?.AppUser == null)
            {
                return ServiceResult<PassengerProfileDto>.Failure($"User profile not found for ID: {id}");
            }

            var appUser = passengerProfile.AppUser;

            var dto = new PassengerProfileDto
            {
                Id = appUser.Id,
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                Email = appUser.Email ?? string.Empty,
                UserName = appUser.UserName ?? string.Empty,
                PhoneNumber = appUser.PhoneNumber,
                ProfilePictureUrl = appUser.ProfilePictureUrl,
                DateOfBirth = appUser.DateOfBirth,
                Address = appUser.Address,
                DateCreated = appUser.DateCreated,
                LastLogin = appUser.LastLogin,
                UserType = appUser.UserType,
                FrequentFlyerId = passengerProfile.FrequentFlyerId,
                KrisFlyerTier = passengerProfile.KrisFlyerTier
            };
            return ServiceResult<PassengerProfileDto>.Success(dto);
        }

        /// <summary>
        /// Gets the profile DTO for a specific Pilot.
        /// </summary>
        public async Task<ServiceResult<PilotProfileDto>> GetPilotByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return ServiceResult<PilotProfileDto>.Failure("User ID cannot be empty.");

            var pilotProfile = await _userRepository.GetPilotProfileByUserIdAsync(id);
            if (pilotProfile?.AppUser == null || pilotProfile.CrewMember?.Employee == null)
            {
                return ServiceResult<PilotProfileDto>.Failure($"Pilot profile or associated data not found for ID: {id}");
            }

            var appUser = pilotProfile.AppUser;
            var employee = pilotProfile.CrewMember.Employee;
            var crewMember = pilotProfile.CrewMember;

            var dto = new PilotProfileDto
            {
                Id = appUser.Id,
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                Email = appUser.Email ?? string.Empty,
                UserName = appUser.UserName ?? string.Empty,
                PhoneNumber = appUser.PhoneNumber,
                ProfilePictureUrl = appUser.ProfilePictureUrl,
                DateOfBirth = appUser.DateOfBirth,
                Address = appUser.Address,
                DateCreated = appUser.DateCreated,
                LastLogin = appUser.LastLogin,
                UserType = appUser.UserType,
                EmployeeId = employee.EmployeeId,
                DateOfHire = employee.DateOfHire,
                Salary = employee.Salary, // Consider security
                CrewBaseAirportId = crewMember.CrewBaseAirportId,
                Position = crewMember.Position,
                LicenseNumber = pilotProfile.LicenseNumber,
                TotalFlightHours = pilotProfile.TotalFlightHours,
                AircraftTypeId = pilotProfile.AircraftTypeId, 
                LastSimCheckDate = pilotProfile.LastSimCheckDate,
                AddedById = pilotProfile.AddedById,
                AddedByName = pilotProfile.AddedBy != null ? $"{pilotProfile.AddedBy.FirstName} {pilotProfile.AddedBy.LastName}" : "N/A"
            };

            return ServiceResult<PilotProfileDto>.Success(dto);
        }

        /// <summary>
        /// Gets the profile DTO for a specific Attendant.
        /// </summary>
        public async Task<ServiceResult<AttendantProfileDto>> GetAttendantByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return ServiceResult<AttendantProfileDto>.Failure("User ID cannot be empty.");

            var attendantProfile = await _userRepository.GetAttendantProfileByUserIdAsync(id);
            if (attendantProfile?.AppUser == null || attendantProfile.CrewMember?.Employee == null)
            {
                return ServiceResult<AttendantProfileDto>.Failure($"Attendant profile or associated data not found for ID: {id}");
            }

            var appUser = attendantProfile.AppUser;
            var employee = attendantProfile.CrewMember.Employee;
            var crewMember = attendantProfile.CrewMember;

            var dto = new AttendantProfileDto
            {
                Id = appUser.Id,
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                Email = appUser.Email ?? string.Empty,
                UserName = appUser.UserName ?? string.Empty,
                PhoneNumber = appUser.PhoneNumber,
                ProfilePictureUrl = appUser.ProfilePictureUrl,
                DateOfBirth = appUser.DateOfBirth,
                Address = appUser.Address,
                DateCreated = appUser.DateCreated,
                LastLogin = appUser.LastLogin,
                UserType = appUser.UserType,
                EmployeeId = employee.EmployeeId,
                DateOfHire = employee.DateOfHire,
                Salary = employee.Salary, // Consider security
                CrewBaseAirportId = crewMember.CrewBaseAirportId,
                Position = crewMember.Position,
                AddedById = attendantProfile.AddedById,
                AddedByName = attendantProfile.AddedBy != null ? $"{attendantProfile.AddedBy.FirstName} {attendantProfile.AddedBy.LastName}" : "N/A"
            };
            return ServiceResult<AttendantProfileDto>.Success(dto);
        }

        /// <summary>
        /// Gets the profile DTO for a specific Supervisor.
        /// </summary>
        public async Task<ServiceResult<SupervisorProfileDto>> GetSupervisorByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return ServiceResult<SupervisorProfileDto>.Failure("User ID cannot be empty.");

            var supervisorProfile = await _userRepository.GetSupervisorProfileByUserIdAsync(id);
            if (supervisorProfile?.AppUser == null || supervisorProfile.Employee == null)
            {
                return ServiceResult<SupervisorProfileDto>.Failure($"Supervisor profile not found for ID: {id}");
            }

            var appUser = supervisorProfile.AppUser;
            var employee = supervisorProfile.Employee;

            var dto = new SupervisorProfileDto
            {
                Id = appUser.Id,
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                Email = appUser.Email ?? string.Empty,
                UserName = appUser.UserName ?? string.Empty,
                PhoneNumber = appUser.PhoneNumber,
                ProfilePictureUrl = appUser.ProfilePictureUrl,
                DateOfBirth = appUser.DateOfBirth,
                Address = appUser.Address,
                DateCreated = appUser.DateCreated,
                LastLogin = appUser.LastLogin,
                UserType = appUser.UserType,
                EmployeeId = employee.EmployeeId,
                DateOfHire = employee.DateOfHire,
                Salary = employee.Salary, // Consider security
                ManagedArea = supervisorProfile.ManagedArea, // Assuming you added this
                AddedById = supervisorProfile.AddedById,
                AddedByName = supervisorProfile.AddedBy != null ? $"{supervisorProfile.AddedBy.FirstName} {supervisorProfile.AddedBy.LastName}" : "N/A"
            };
            return ServiceResult<SupervisorProfileDto>.Success(dto);
        }

        #endregion


        #region --- Helper Methods --- 

        /// <summary>
        /// The core method for creating a user and assigning a role.
        /// This method is called by all registration functions.
        /// It creates the AppUser and all related entities (Employee, Admin, Pilot, etc.)
        /// in a single transaction.
        /// </summary>
        private async Task<IdentityResult> CreateUserWithRoleAsync(AppUser user, string password, string role)
            {
                // The magic happens here: _userManager.CreateAsync(user, ...) will
                // automatically save the 'user' AND all related navigation properties
                // (like user.Admin, user.Employee, user.Pilot) in a single transaction.
                var result = await _unitOfWork.UserManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    return result; // Return failure
                }

                // Ensure the role exists
                if (!await _unitOfWork.RoleManager.RoleExistsAsync(role))
                {
                    await _unitOfWork.RoleManager.CreateAsync(new IdentityRole(role));
                }

                // Add user to the role
                await _unitOfWork.UserManager.AddToRoleAsync(user, role);

                return result; // Return success
            }

            /// <summary>
            /// Generates a secure random password for new employee accounts.
            /// </summary>
            private static string GenerateRandomPassword(int length = 12)
            {
                const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
                const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                const string numbers = "0123456789";
                const string specials = "!@#$%^&*()_-+=<>?";

                var allChars = lowerChars + upperChars + numbers + specials;
                var random = new Random();
                var password = new StringBuilder();

                // Ensure at least one of each type
                password.Append(lowerChars[random.Next(lowerChars.Length)]);
                password.Append(upperChars[random.Next(upperChars.Length)]);
                password.Append(numbers[random.Next(numbers.Length)]);
                password.Append(specials[random.Next(specials.Length)]);

                // Fill the rest of the password length
                for (int i = 0; i < length - 4; i++)
                {
                    password.Append(allChars[random.Next(allChars.Length)]);
                }

                // Shuffle the characters to avoid predictable pattern
                return new string(password.ToString().ToCharArray().OrderBy(x => random.Next()).ToArray());
            }

            /// <summary>
            /// Validates the authenticated user (caller) and ensures they have the required role.
            /// </summary>
            /// <returns>A tuple containing a ServiceResult and the caller's UserId if successful.</returns>
            private async Task<(ServiceResult IsSuccess, string? UserId)> AuthorizeCallerAsync(ClaimsPrincipal user, string requiredRole)
            {
                if (user == null || !user.Identity.IsAuthenticated)
                {
                    return (ServiceResult.Failure("Authentication required."), null);
                }

                var userId = _unitOfWork.UserManager.GetUserId(user);
                if (string.IsNullOrEmpty(userId))
                {
                    return (ServiceResult.Failure("User ID not found in token."), null);
                }

                if (!user.IsInRole(requiredRole))
                {
                    return (ServiceResult.Failure($"Access denied. User is not in the '{requiredRole}' role."), null);
                }

                // Optional: Double-check user exists in DB
                var appUser = await _unitOfWork.UserManager.FindByIdAsync(userId);
                if (appUser == null)
                {
                    return (ServiceResult.Failure("Authenticated user record not found in database."), null);
                }

                return (ServiceResult.Success(), userId);
            }


        /// <summary>
        /// Centralized logic to authorize an update action based on user roles.
        /// Checks if the current user has permission to update the target user's profile.
        /// </summary>
        /// <param name="currentUserPrincipal">ClaimsPrincipal of the user making the request.</param>
        /// <param name="targetUserId">The ID of the user whose profile is being updated.</param>
        /// <param name="targetRole">The UserType of the user being updated.</param>
        /// <returns>A ServiceResult indicating success or failure with error messages.</returns>
        private async Task<ServiceResult> AuthorizeUpdateAsync(ClaimsPrincipal currentUserPrincipal, string targetUserId, UserType targetRole)
        {
            var currentUserId = await _userRepository.GetUserIdFromClaimsPrincipalAsync(currentUserPrincipal);
            if (string.IsNullOrEmpty(currentUserId))
            {
                // Should not happen if [Authorize] attribute is used correctly, but good for safety.
                return ServiceResult.Failure("Authentication required. Unable to identify current user.");
            }

            // Rule 1: Users can always update their own profile.
            if (currentUserId == targetUserId)
            {
                return ServiceResult.Success();
            }

            // Rule 2: SuperAdmin can update anyone *except* another SuperAdmin.
            // (We prevent SuperAdmins from modifying each other for security/simplicity).
            if (currentUserPrincipal.IsInRole("SuperAdmin") && targetRole != UserType.SuperAdmin)
            {
                // Optional: Add company check here if applicable in the future.
                return ServiceResult.Success();
            }

            // Rule 3: Admin can update Pilots and Attendants.
            // (Assuming Admins manage flight crew within their scope).
            if (currentUserPrincipal.IsInRole("Admin") && (targetRole == UserType.Pilot || targetRole == UserType.Attendant))
            {
                // Optional: Add checks here if Admins belong to specific bases/regions
                // and should only update crew within their scope.
                return ServiceResult.Success();
            }
  
            // If none of the above rules apply, deny access.
            return ServiceResult.Failure("Authorization denied. You do not have permission to update this user's profile.");
        }

        
        /// <summary>
        /// Updates AppUser fields if DTO values are provided and different.
        /// Returns true if any changes were made.
        /// </summary>
        private bool UpdateAppUserFromDto(AppUser appUser, string? firstName, string? lastName, string? phone, DateTime? dob, string? address)
        {
            bool changed = false;
            if (!string.IsNullOrWhiteSpace(firstName) && appUser.FirstName != firstName)
            {
                appUser.FirstName = firstName;
                changed = true;
            }
            if (!string.IsNullOrWhiteSpace(lastName) && appUser.LastName != lastName)
            {
                appUser.LastName = lastName;
                changed = true;
            }
            if (!string.IsNullOrWhiteSpace(phone) && appUser.PhoneNumber != phone)
            {
                appUser.PhoneNumber = phone;
                changed = true;
            }
            if (dob.HasValue && appUser.DateOfBirth != dob.Value)
            {
                appUser.DateOfBirth = dob.Value;
                changed = true;
            }
            if (address != null && appUser.Address != address) // Allow empty string for address
            {
                appUser.Address = address;
                changed = true;
            }
            return changed;
        }

        /// <summary>
        /// Updates Employee fields if DTO values are provided and different.
        /// Returns true if any changes were made.
        /// </summary>
        private bool UpdateEmployeeFromDto(Employee? employee, DateTime? dateOfHire, decimal? salary)
        {
            if (employee == null) return false;
            bool changed = false;
            if (dateOfHire.HasValue && employee.DateOfHire != dateOfHire.Value)
            {
                employee.DateOfHire = dateOfHire.Value;
                changed = true;
            }
            if (salary.HasValue && employee.Salary != salary.Value)
            {
                employee.Salary = salary.Value;
                changed = true;
            }
            return changed;
        }

        /// <summary>
        /// Updates CrewMember fields if DTO values are provided and different.
        /// Returns true if any changes were made.
        /// </summary>
        private bool UpdateCrewMemberFromDto(CrewMember? crewMember, string? crewBaseAirportId)
        {
            if (crewMember == null) return false;
            bool changed = false;
            if (!string.IsNullOrWhiteSpace(crewBaseAirportId) && crewMember.CrewBaseAirportId != crewBaseAirportId)
            {
                crewMember.CrewBaseAirportId = crewBaseAirportId;
                changed = true;
            }
            return changed;
        }

        /// <summary>
        /// Updates Pilot specific fields if DTO values are provided and different.
        /// Returns true if any changes were made.
        /// </summary>
        private bool UpdatePilotSpecificFromDto(Pilot pilotProfile, string? licenseNumber, int? totalFlightHours, DateTime? lastSimCheckDate)
        {
            bool changed = false;
            if (!string.IsNullOrWhiteSpace(licenseNumber) && pilotProfile.LicenseNumber != licenseNumber)
            {
                pilotProfile.LicenseNumber = licenseNumber;
                changed = true;
            }
            if (totalFlightHours.HasValue && pilotProfile.TotalFlightHours != totalFlightHours.Value)
            {
                pilotProfile.TotalFlightHours = totalFlightHours.Value;
                changed = true;
            }
            if (lastSimCheckDate.HasValue && pilotProfile.LastSimCheckDate != lastSimCheckDate.Value)
            {
                pilotProfile.LastSimCheckDate = lastSimCheckDate.Value;
                changed = true;
            }
            return changed;
        }

        // --- Implement similar specific update helpers for Admin, Supervisor, Attendant if they have unique fields ---
        private bool UpdateAdminSpecificFromDto(Admin adminProfile, string? department)
        {
            bool changed = false;
            if (department != null && adminProfile.Department != department) // Assuming Admin has Department
            {
                adminProfile.Department = department; // You need to add Department to Admin entity
                changed = true;
            }
            return changed;
        }

        private bool UpdateSupervisorSpecificFromDto(Supervisor supervisorProfile, string? managedArea)
        {
            bool changed = false;
            if (managedArea != null && supervisorProfile.ManagedArea != managedArea) // Assuming Supervisor has ManagedArea
            {
                supervisorProfile.ManagedArea = managedArea; // You need to add ManagedArea to Supervisor entity
                changed = true;
            }
            return changed;
        }

        /// <summary>
        /// Handles profile picture update: saves new, deletes old, manages rollback path.
        /// </summary>
        /// <returns>A tuple: (ServiceResult, oldPicturePath, newPicturePath)</returns>
        private async Task<(ServiceResult Result, string? OldPath, string? NewPath)> HandleProfilePictureUpdate(AppUser appUser, IFormFile? newPicture)
        {
            string? oldPicturePath = appUser.ProfilePictureUrl;
            string? newPicPath = null;

            if (newPicture == null)
            {
                return (ServiceResult.Success(), oldPicturePath, null); // No new picture provided
            }

            try
            {
                newPicPath = await _fileService.SaveFileAsync(newPicture, "ProfilePictures");
                appUser.ProfilePictureUrl = newPicPath; // Update path in memory

                // Don't delete the old picture yet, wait for successful save
                return (ServiceResult.Success(), oldPicturePath, newPicPath);
            }
            catch (Exception ex)
            {
                // Failed to save the new picture
                appUser.ProfilePictureUrl = oldPicturePath; // Revert path in memory
                return (ServiceResult.Failure($"Failed to save profile picture: {ex.Message}"), oldPicturePath, null);
            }
        }


        #endregion

    }
}
