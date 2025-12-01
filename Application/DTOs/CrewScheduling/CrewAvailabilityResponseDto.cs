namespace Application.DTOs.CrewScheduling
{
    // Represents a potentially available crew member.
    public class CrewAvailabilityResponseDto
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string CrewBaseAirportIata { get; set; } = string.Empty;
        public bool IsTypeRated { get; set; } = true; // Relevant for pilots
        public bool HasValidCertification { get; set; } = true; // Basic check
        // Add more fields like recent flight hours if needed for FDTL
    }
}