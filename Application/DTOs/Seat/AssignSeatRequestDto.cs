using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Seat
{
    // DTO for assigning a specific seat to a passenger within a booking.
    public class AssignSeatRequestDto
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        public int PassengerId { get; set; } // The passenger getting the seat

        [Required]
        [StringLength(20)]
        public string SeatId { get; set; } = string.Empty; // The specific seat ID to assign
    }
}