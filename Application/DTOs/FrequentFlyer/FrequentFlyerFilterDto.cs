namespace Application.DTOs.FrequentFlyer
{
    // DTO for filtering frequent flyer accounts.
    public class FrequentFlyerFilterDto
    {
        public string? CardNumberContains { get; set; }
        public string? Level { get; set; }
        public int? MinPoints { get; set; }
        public int? MaxPoints { get; set; }
        public bool IncludeDeleted { get; set; } = false;
    }
}
