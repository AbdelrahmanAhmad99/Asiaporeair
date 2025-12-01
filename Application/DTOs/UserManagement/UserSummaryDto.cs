using Domain.Enums;

namespace Application.DTOs.UserManagement
{
    // A lightweight DTO for displaying users in lists and search results.
    public class UserSummaryDto
    {
        public string Id { get; set; } = string.Empty; // The AppUser ID (string)
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserType UserType { get; set; }
        public bool IsActive { get; set; } = true;
    }
}