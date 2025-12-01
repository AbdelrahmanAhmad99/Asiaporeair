using System.Text.Json.Serialization;

namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// DTO for deserializing Payment data from a JSON file.
    /// Matches the properties of the Payment entity for data transfer, ensuring high professionalism.
    /// </summary>
    public class PaymentSeedDto
    {
        [JsonPropertyName("booking_fk")]
        public int BookingId { get; set; }

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }

        [JsonPropertyName("method")]
        public string Method { get; set; } = string.Empty;

        [JsonPropertyName("transaction_datetime")]
        public DateTime TransactionDateTime { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "Success";  

        [JsonPropertyName("transaction_id")]
        public string? TransactionId { get; set; }

        [JsonPropertyName("IsDeleted")]
        public bool IsDeleted { get; set; }
    }
}