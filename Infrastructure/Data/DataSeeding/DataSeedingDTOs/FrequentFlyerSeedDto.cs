namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// DTO for seeding FrequentFlyer data.
    /// </summary>
    public class FrequentFlyerSeedDto
    {
        // The unique card number
        public string CardNumber { get; set; } = string.Empty;

        // The flyer's membership level (e.g., Gold, Platinum)
        public string? Level { get; set; }

        // Initial award points balance
        public int? AwardPoints { get; set; }
    }
}