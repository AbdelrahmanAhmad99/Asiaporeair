using Domain.Enums;

namespace Application.DTOs.Crew
{
    // DTO for filtering crew members.
    public class CrewFilterDto
    {
        public string? NameContains { get; set; }
        public string? Position { get; set; } // "Pilot" or "Attendant"
        public string? CrewBaseAirportIata { get; set; }
        public bool? HasExpiredCertification { get; set; }
        public bool IncludeDeleted { get; set; } = false;
    }
}