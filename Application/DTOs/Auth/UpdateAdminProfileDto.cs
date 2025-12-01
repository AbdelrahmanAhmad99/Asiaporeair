using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    /// <summary>
    /// DTO for updating an Admin profile.
    /// </summary>
    public class UpdateAdminProfileDto : UpdateEmployeeProfileDto
    { 
        [StringLength(50)]
        public string? Department { get; set; }
    }
}