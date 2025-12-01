using System.Text.Json.Serialization;

namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// Data Transfer Object for AncillarySale seeding.
    /// Maps directly to the JSON file structure, using [JsonPropertyName] for clean deserialization.
    /// </summary>
    public class AncillarySaleSeedDto
    {
        [JsonPropertyName("booking_fk")]
        public int BookingId { get; set; }

        [JsonPropertyName("product_fk")]
        public int ProductId { get; set; }

        [JsonPropertyName("quantity")]
        public int? Quantity { get; set; } // Nullable, as it is 'int?' in the entity

        [JsonPropertyName("price_paid")]
        public decimal? PricePaid { get; set; } // Nullable, as it is 'decimal?' in the entity

        [JsonPropertyName("segment_fk")]
        public int? SegmentId { get; set; } // Nullable, as it is 'int?' in the entity

        [JsonPropertyName("IsDeleted")]
        public bool IsDeleted { get; set; } = false;
    }
}