namespace Application.DTOs.Auth
{
    /// <summary>
    /// Profile DTO for a SuperAdmin user. Includes Employee data.
    /// </summary>
    public class SuperAdminProfileDto : UserProfileBaseDto
    {
        // Fields from the 'Employee' entity
        public int EmployeeId { get; set; }
        public DateTime? DateOfHire { get; set; }
        public decimal? Salary { get; set; } // Consider if this should be exposed

        // No specific fields in SuperAdmin entity currently, besides FKs
    }
}