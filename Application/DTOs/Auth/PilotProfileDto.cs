namespace Application.DTOs.Auth
{
    /// <summary>
    /// Profile DTO for a Pilot user. Includes Employee, CrewMember, and Pilot data.
    /// </summary>
    public class PilotProfileDto : UserProfileBaseDto
    {
        // --- Fields from 'Employee' ---
        public int EmployeeId { get; set; }
        public DateTime? DateOfHire { get; set; }
        public decimal? Salary { get; set; } // Consider exposure

        // --- Fields from 'CrewMember' ---
        public string? CrewBaseAirportId { get; set; } // IATA Code
        public string? Position { get; set; } // Should be "Pilot"

        // --- Fields from 'Pilot' ---
        public string LicenseNumber { get; set; } = string.Empty;
        public int? TotalFlightHours { get; set; }
        public int AircraftTypeId { get; set; } // Type Rating FK
        // You might want to include AircraftType Model/Manufacturer here by joining
        // public string? AircraftTypeModel { get; set; }
        public DateTime? LastSimCheckDate { get; set; }
        public string? AddedById { get; set; }
        public string? AddedByName { get; set; }
    }
}