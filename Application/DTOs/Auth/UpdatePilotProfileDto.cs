using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    /// <summary>
    /// DTO for updating a Pilot profile. Includes fields from Pilot and CrewMember tables.
    /// </summary>
    public class UpdatePilotProfileDto : UpdateEmployeeProfileDto
    {
        // --- CrewMember Fields ---
        /// <summary>
        /// The IATA code of the pilot's base airport.
        /// </summary>
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Crew base airport must be a 3-letter IATA code.")]
        public string? CrewBaseAirportId { get; set; } // Matches crew_member.crew_base_airport_fk

        // --- Pilot Specific Fields ---
        /// <summary>
        /// Pilot's license number.
        /// </summary>
        [StringLength(20)]
        public string? LicenseNumber { get; set; } // Matches pilot.license_number

        /// <summary>
        /// Total flight hours accumulated by the pilot.
        /// </summary>
        [Range(0, int.MaxValue, ErrorMessage = "Total flight hours cannot be negative.")]
        public int? TotalFlightHours { get; set; } // Matches pilot.total_flight_hours

        /// <summary>
        /// Date of the pilot's last simulator check.
        /// </summary>
        public DateTime? LastSimCheckDate { get; set; } // Matches pilot.last_sim_check_date

        // Note: type_rating_fk (AircraftTypeId) might be managed separately,
        // as changing it often involves significant certification/training updates.
        // It's often not part of a standard profile update.
    }
}