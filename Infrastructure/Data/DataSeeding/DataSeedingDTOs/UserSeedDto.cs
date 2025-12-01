namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// DTO for seeding User data. Represents a customer account linked to an Identity user.
    /// Depends on AppUser (Identity) and FrequentFlyer.
    /// </summary>
    public class UserSeedDto
    {
        // Primary Key is automatically generated (UserId), so we focus on Foreign Keys

        // Foreign Key to AppUser (Identity)
        public string AppUserId { get; set; } = string.Empty;

        // Foreign Key to FrequentFlyer Entity (FlyerId) (optional)
        public int? FrequentFlyerId { get; set; }

        // The tier in the frequent flyer program (e.g., 'Gold', 'Silver')
        public string? KrisFlyerTier { get; set; }
    }
}