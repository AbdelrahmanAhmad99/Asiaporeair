using System.Text.Json.Serialization;
using System;

namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// Data Transfer Object for PriceOfferLog entity seeding.
    /// Used to professionally deserialize data from the PriceOfferLog.json file.
    /// It includes all non-identity fields required for the entity creation,
    /// adhering to the foreign key dependencies.
    /// </summary>
    public class PriceOfferLogSeedDto
    { 
        [JsonPropertyName("ProductId")]
        public int? ProductId { get; set; }
         
        [JsonPropertyName("OfferPriceQuote")]
        public decimal OfferPriceQuote { get; set; }
         
        [JsonPropertyName("Timestamp")]
        public DateTime Timestamp { get; set; }
         
        [JsonPropertyName("ContextAttributesId")]
        public int ContextAttributesId { get; set; }
         
        [JsonPropertyName("FareId")]
        public string? FareId { get; set; }
         
        [JsonPropertyName("AncillaryId")]
        public int? AncillaryId { get; set; } 
        [JsonPropertyName("IsDeleted")]
        public bool IsDeleted { get; set; }
    }
}