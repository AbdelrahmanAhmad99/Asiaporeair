namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// DTO for seeding core Identity user data (AspNetUsers).
    /// </summary>
    public class AppUserSeedDto
    {
        // User's username (e.g., email or unique identifier for login)
        public string UserName { get; set; } = string.Empty;

        // User's email address
        public string Email { get; set; } = string.Empty;

        // User's password (will be hashed during seeding process)
        public string Password { get; set; } = string.Empty;

        // User's first name
        public string FirstName { get; set; } = string.Empty;

        // User's last name
        public string LastName { get; set; } = string.Empty;

        // User's date of birth (optional)
        public DateTime? DateOfBirth { get; set; }

        // User's residential address (optional)
        public string Address { get; set; } = string.Empty;

        // User's mobile phone number
        public string? PhoneNumber { get; set; }

        // Discriminator for user type (e.g., Customer, Employee, Admin)
        public string UserType { get; set; } = string.Empty;

        // Initial Role to be assigned (e.g., Customer, Admin, Pilot)
        public string Role { get; set; } = string.Empty;
    }
}