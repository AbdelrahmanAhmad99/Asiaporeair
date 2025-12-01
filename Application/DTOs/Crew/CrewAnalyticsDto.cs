using System.Collections.Generic;

namespace Application.DTOs.Crew
{
    // DTO for crew-specific analytics.
    public class CrewAnalyticsDto
    {
        public int TotalActiveCrew { get; set; }
        public int TotalPilots { get; set; }
        public int TotalAttendants { get; set; }
        public Dictionary<string, int> CrewCountByBase { get; set; } = new Dictionary<string, int>();
        public int CertificationsExpiringSoon { get; set; } // e.g., within 30 days
        public int CertificationsExpired { get; set; }
    }
}