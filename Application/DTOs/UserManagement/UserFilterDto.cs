using Domain.Enums;

namespace Application.DTOs.UserManagement
{
    // DTO for filtering the paginated user list.
    public class UserFilterDto
    {
        public string? NameContains { get; set; }
        public string? EmailContains { get; set; }
        public UserType? UserType { get; set; }
        public bool IncludeDeleted { get; set; } = false;
    }
}