using System;
using Domain.Enums;

namespace Application.DTOs.Employee
{
    // A summary DTO for displaying employees in lists or search results.
    public class EmployeeSummaryDto
    {
        public int EmployeeId { get; set; }
        public string AppUserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public UserType Role { get; set; }
        public string Salary { get; set; } = string.Empty;
        public string Position { get; set; } = "N/A"; // e.g., "Pilot", "Attendant", "Admin"
        public string? CrewBaseAirportIata { get; set; }
        public DateTime? DateOfHire { get; set; }
        public bool IsActive { get; set; } = true;
    }
}