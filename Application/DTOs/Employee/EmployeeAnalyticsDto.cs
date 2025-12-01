using System.Collections.Generic;

namespace Application.DTOs.Employee
{
    // DTO for the HR Management Dashboard analytics widgets.
    public class EmployeeAnalyticsDto
    {
        public int TotalActiveEmployees { get; set; }
        public int TotalPilots { get; set; }
        public int TotalAttendants { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalSupervisors { get; set; }
        public decimal AverageSalary { get; set; }
        public Dictionary<string, int> EmployeesByBase { get; set; } = new Dictionary<string, int>();
    }
}