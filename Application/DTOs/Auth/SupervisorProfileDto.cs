namespace Application.DTOs.Auth
{
    /// <summary>
    /// Profile DTO for a Supervisor user. Includes Employee data.
    /// </summary>
    public class SupervisorProfileDto : UserProfileBaseDto
    {
        // Fields from the 'Employee' entity
        public int EmployeeId { get; set; }
        public DateTime? DateOfHire { get; set; }
        public decimal? Salary { get; set; } // Consider if this should be exposed

        // Fields from the 'Supervisor' entity
        public string? ManagedArea { get; set; } // Assuming you added this
        public string? AddedById { get; set; }
        public string? AddedByName { get; set; } // Populated from AddedBy relation
    }
}