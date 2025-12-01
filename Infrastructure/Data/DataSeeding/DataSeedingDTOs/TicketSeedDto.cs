using System.Text.Json.Serialization;

namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// Data Transfer Object for deserializing Ticket data from JSON seed file.
    /// Maps directly to the 'Ticket' entity properties.
    /// </summary>
    public class TicketSeedDto
    {
        // Unique ticket code (e.g., ASPR-T00123)
        [JsonPropertyName("ticket_code")]
        public string TicketCode { get; set; } = string.Empty;

        // Date the ticket was issued (DateTime2)
        [JsonPropertyName("issue_date")]
        public DateTime IssueDate { get; set; }

        // Current status of the ticket (e.g., 'Issued', 'Boarded', 'Cancelled')
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        // FK to the Passenger entity (1 - 40)
        [JsonPropertyName("passenger_id_fk")]
        public int PassengerId { get; set; }

        // FK to the Booking entity (1 - 40)
        [JsonPropertyName("booking_id_fk")]
        public int BookingId { get; set; }

        // FK to the specific FlightInstance (1 - 30)
        [JsonPropertyName("flight_instance_id_fk")]
        public int FlightInstanceId { get; set; }

        // FK to the Seat entity (string - PK of Seat table, Nullable in DB)
        [JsonPropertyName("seat_id_fk")]
        public string? SeatId { get; set; }

        // FK to the FrequentFlyer program (1 - 9, Nullable)
        [JsonPropertyName("frequent_flyer_id_fk")]
        public int? FrequentFlyerId { get; set; }

        // Soft deletion flag
        [JsonPropertyName("is_deleted")]
        public bool IsDeleted { get; set; } = false;
    }
}