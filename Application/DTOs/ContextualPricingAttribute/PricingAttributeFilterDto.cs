namespace Application.DTOs.ContextualPricingAttribute
{
    // DTO for filtering searches for pricing attributes
    public class PricingAttributeFilterDto
    {
        public int? MinTimeUntilDeparture { get; set; }
        public int? MaxTimeUntilDeparture { get; set; }
        public int? MinLengthOfStay { get; set; }
        public int? MaxLengthOfStay { get; set; }
        public bool IncludeDeleted { get; set; } = false;
    }
}