using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    public class AttendantDto : EmployeeRegisterDtoBase
    {
        // --- Crew member properties ---
        [Required]
        [StringLength(3)]
        public string CrewBaseAirport { get; set; } = string.Empty;

        // Add any other attendant-specific properties here
        // e.g., public List<string> Languages { get; set; }
    }
}