using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth
{
    public class PilotDto : EmployeeRegisterDtoBase
    {
        // Employee properties like DateOfHire and Salary are now in the base class.

        // --- Crew member properties ---
        [Required]
        [StringLength(3)]
        public string CrewBaseAirport { get; set; } = string.Empty;

        // --- Pilot-specific properties ---
        [Required]
        public string LicenseNumber { get; set; } = string.Empty;
        public int? TotalFlightHours { get; set; }
        [Required]
        public int AircraftTypeId { get; set; }
        public DateTime? LastSimCheckDate { get; set; }
    }
}