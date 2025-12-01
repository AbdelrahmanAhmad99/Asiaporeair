using System;
using Domain.Enums; // Assuming UserType enum is here

namespace Application.DTOs.Crew
{
    // A summary DTO for listing crew members.
    public class CrewMemberSummaryDto
    {
        public int EmployeeId { get; set; }
        public string AppUserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty; // "Pilot" or "Attendant"
        public string CrewBaseAirportIata { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}