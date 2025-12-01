using System;
using System.Text.Json.Serialization;

namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// DTO for deserializing ContextualPricingAttributes data from JSON.
    /// This entity holds attributes that influence dynamic pricing decisions.
    /// </summary>
    public class ContextualPricingAttributesSeedDto
    {
        // Note: AttributeId (PK) is typically auto-generated (IDENTITY) in the database.
        // It is omitted here but is tracked implicitly by the Seeder's execution order.
         
        [JsonPropertyName("time_until_departure")]
        public int? TimeUntilDeparture { get; set; }
         
        [JsonPropertyName("length_of_stay")]
        public int? LengthOfStay { get; set; }
         
        [JsonPropertyName("competitor_fares")]
        public string CompetitorFares { get; set; }
         
        [JsonPropertyName("willingness_to_pay")]
        public decimal? WillingnessToPay { get; set; }
         
        [JsonPropertyName("IsDeleted")]
        public bool IsDeleted { get; set; } = false;
    }
}