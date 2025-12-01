namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// Unified DTO for reading Employee-type user data from a single JSON file.
    /// This combines data needed for AppUser, Employee, and the specific Role entity (e.g., Admin/SuperAdmin).
    /// </summary>
    public class EmployeeSeedInputDto
    {
        // AppUserSeedDto Properties (Identity Core)
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime? DateOfBirth { get; set; }
        public string Address { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string UserType { get; set; } = string.Empty; // e.g., "Employee"
        public string Role { get; set; } = string.Empty;  
         
        public DateTime? DateOfHire { get; set; }
        public decimal? Salary { get; set; }
        public int? ShiftPreferenceId { get; set; }  
         
        public string? Department { get; set; }
         
        public string? AddedById { get; set; }
    }
}