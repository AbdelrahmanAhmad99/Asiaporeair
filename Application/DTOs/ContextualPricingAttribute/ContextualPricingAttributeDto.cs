namespace Application.DTOs.ContextualPricingAttribute
{
    // DTO for representing a set of contextual pricing attributes
    public class ContextualPricingAttributeDto
    {
        public int AttributeId { get; set; }
        public int? TimeUntilDeparture { get; set; } // e.g., 30 (days)
        public int? LengthOfStay { get; set; } // e.g., 7 (days)
        public string? CompetitorFares { get; set; } // JSON or serialized data of competitor prices
        public decimal? WillingnessToPay { get; set; } // A factor or absolute value
    }
}