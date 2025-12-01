using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    /// <summary>
    /// DTO for updating a Flight Attendant profile. Includes fields from CrewMember table.
    /// </summary>
    public class UpdateAttendantProfileDto : UpdateEmployeeProfileDto
    {
        // --- CrewMember Fields --- 
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Crew base airport must be a 3-letter IATA code.")]
        public string? CrewBaseAirportId { get; set; } // Matches crew_member.crew_base_airport_fk

        // Add any other Attendant-specific updatable fields here, e.g.:
        // public List<string>? LanguagesSpoken { get; set; }
        // public string? SeniorityLevel { get; set; }
    }
}