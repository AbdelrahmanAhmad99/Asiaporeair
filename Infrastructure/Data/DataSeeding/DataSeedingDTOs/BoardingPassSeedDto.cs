using System;
using System.Text.Json.Serialization;

namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// Data Transfer Object for BoardingPass entity seeding, ensuring professional JSON property mapping.
    /// Used by the BoardingPassSeeder to deserialize data from BoardingPass.json.
    /// </summary>
    public class BoardingPassSeedDto
    {
        // Composite Foreign Key part 1 to BookingPassenger entity (booking_id)
        [JsonPropertyName("booking_passenger_booking_id")]
        public int BookingPassengerBookingId { get; set; }

        // Composite Foreign Key part 2 to BookingPassenger entity (passenger_id)
        [JsonPropertyName("booking_passenger_passenger_id")]
        public int BookingPassengerPassengerId { get; set; }

        // Foreign Key to the Seat entity, representing the assigned seat
        [JsonPropertyName("seat_fk")]
        public string SeatId { get; set; } = string.Empty;

        // The actual time the passenger boarded (nullable in DB)
        [JsonPropertyName("boarding_time")]
        public DateTime? BoardingTime { get; set; }

        // Status for expedited screening (e.g., Precheck status - nullable in DB)
        [JsonPropertyName("precheck_status")]
        public bool? PrecheckStatus { get; set; }

        // Soft delete flag, required field with default value
        [JsonPropertyName("IsDeleted")]
        public bool IsDeleted { get; set; }
    }
}