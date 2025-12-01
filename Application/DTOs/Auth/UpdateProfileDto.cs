using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    /// <summary>
    /// Base DTO for updating common user profile information.
    /// All fields are optional to allow partial updates.
    /// </summary>
    public abstract class UpdateProfileDto
    {
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters.")]
        public string? FirstName { get; set; }

        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters.")]
        public string? LastName { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// New profile picture file to upload. If provided, the old one will be replaced.
        /// </summary>
        public IFormFile? ProfilePicture { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(255, ErrorMessage = "Address cannot exceed 255 characters.")]
        public string? Address { get; set; }
    }
}