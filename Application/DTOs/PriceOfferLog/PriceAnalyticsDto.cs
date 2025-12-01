namespace Application.DTOs.PriceOfferLog
{
    // DTO to hold aggregated pricing analytics.
    public class PriceAnalyticsDto
    {
        public string ItemCode { get; set; } = string.Empty; // Fare Code or Ancillary Product ID
        public decimal AveragePrice { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int OfferCount { get; set; }
        // Could add Standard Deviation, etc.
    }
}