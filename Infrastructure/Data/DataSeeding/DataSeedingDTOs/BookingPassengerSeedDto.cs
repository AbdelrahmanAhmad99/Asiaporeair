using System.Text.Json.Serialization;

namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// Data Transfer Object for seeding the BookingPassenger entity.
    /// It mirrors the primary keys (BookingId, PassengerId) and the SeatAssignmentId foreign key.
    /// </summary>
    public class BookingPassengerSeedDto
    {
        [JsonPropertyName("booking_id")]
        public int BookingId { get; set; }

        [JsonPropertyName("passenger_id")]
        public int PassengerId { get; set; }

        [JsonPropertyName("seat_assignment_fk")]
        public string? SeatAssignmentFk { get; set; }  
    }
}