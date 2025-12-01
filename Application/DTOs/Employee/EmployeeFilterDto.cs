using Domain.Enums;

namespace Application.DTOs.Employee
{
    // DTO for advanced filtering of employees in the management dashboard.
    public class EmployeeFilterDto
    {
        public string? NameContains { get; set; }
        public UserType? Role { get; set; }
        public string? CrewBaseAirportIata { get; set; }
        public bool IncludeDeleted { get; set; } = false;
    }
}