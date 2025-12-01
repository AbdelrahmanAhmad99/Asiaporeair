using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Seat
{
    // DTO for requesting available seats on a specific flight.
    public class SeatAvailabilityRequestDto
    {
        [Required]
        public int FlightInstanceId { get; set; }
        // Optionally filter by cabin class
        public int? CabinClassId { get; set; }
    }
}