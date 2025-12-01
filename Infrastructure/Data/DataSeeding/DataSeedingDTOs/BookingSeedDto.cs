using System;
using System.Text.Json.Serialization;

namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// Data Transfer Object for seeding the Booking entity.
    /// This DTO is designed to professionally deserialize data from the Booking.json file,
    /// ensuring type and naming compatibility with the Domain.Entities.Booking model.
    /// </summary>
    public class BookingSeedDto
    {
        // Note: booking_id is an IDENTITY column and is therefore excluded from the DTO.

        [JsonPropertyName("user_fk")]
        public int UserId { get; set; }

        [JsonPropertyName("flight_instance_fk")]
        public int FlightInstanceId { get; set; }

        [JsonPropertyName("booking_ref")]
        public string BookingRef { get; set; }

        [JsonPropertyName("booking_time")]
        public DateTime BookingTime { get; set; }

        [JsonPropertyName("price_total")]
        public decimal? PriceTotal { get; set; }

        [JsonPropertyName("payment_status")]
        public string PaymentStatus { get; set; }

        [JsonPropertyName("fare_basis_code_fk")]
        public string FareBasisCodeId { get; set; }

        [JsonPropertyName("PointsAwarded")]
        public bool PointsAwarded { get; set; }

        [JsonPropertyName("IsDeleted")]
        public bool IsDeleted { get; set; } = false;
    }
}