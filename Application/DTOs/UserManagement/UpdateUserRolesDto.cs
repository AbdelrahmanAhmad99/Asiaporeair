using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.UserManagement
{
    // DTO for updating the roles (permissions) of a user.
    public class UpdateUserRolesDto
    {
        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public List<string> Roles { get; set; } = new List<string>(); // e.g., ["Admin", "CheckInAgent"]
    }
}