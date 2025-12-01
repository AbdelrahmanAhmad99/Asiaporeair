namespace Application.DTOs.Auth
{
    /// <summary>
    /// Profile DTO for an Admin user. Includes Employee data.
    /// </summary>
    public class AdminProfileDto : UserProfileBaseDto
    {
        // Fields from the 'Employee' entity
        public int EmployeeId { get; set; }
        public DateTime? DateOfHire { get; set; }
        public decimal? Salary { get; set; } // Consider if this should be exposed

        // Fields from the 'Admin' entity
        public string? Department { get; set; } // Assuming you added this
        public string? AddedById { get; set; }
        public string? AddedByName { get; set; } // Populated from AddedBy relation
    }
}