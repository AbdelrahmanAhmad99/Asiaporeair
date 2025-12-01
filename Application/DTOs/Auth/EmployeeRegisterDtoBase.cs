using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    public abstract class EmployeeRegisterDtoBase
    {
        // --- Required Fields ---
        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        // --- Employee-Specific Fields ---
        public DateTime? DateOfHire { get; set; }
        public decimal? Salary { get; set; }

        // --- Optional Fields ---
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public IFormFile? ProfilePicture { get; set; }
    }
}