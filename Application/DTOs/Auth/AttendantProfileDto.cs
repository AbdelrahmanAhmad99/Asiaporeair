namespace Application.DTOs.Auth
{
    /// <summary>
    /// Profile DTO for an Attendant user. Includes Employee and CrewMember data.
    /// </summary>
    public class AttendantProfileDto : UserProfileBaseDto
    {
        // --- Fields from 'Employee' ---
        public int EmployeeId { get; set; }
        public DateTime? DateOfHire { get; set; }
        public decimal? Salary { get; set; } // Consider exposure

        // --- Fields from 'CrewMember' ---
        public string? CrewBaseAirportId { get; set; } // IATA Code
        public string? Position { get; set; } // Should be "Attendant"

        // --- Fields from 'Attendant' ---
        public string? AddedById { get; set; }
        public string? AddedByName { get; set; }

        // Add any other attendant-specific fields from your model if they exist
        // public List<string>? LanguagesSpoken { get; set; }
    }
}