using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    /// <summary>
    /// DTO for updating a Supervisor profile.
    /// </summary>
    public class UpdateSupervisorProfileDto : UpdateEmployeeProfileDto
    {
        /// <summary>
        /// Optional: Specific area or team the Supervisor manages.
        /// </summary>
        [StringLength(100)]
        public string? ManagedArea { get; set; }
    }
}