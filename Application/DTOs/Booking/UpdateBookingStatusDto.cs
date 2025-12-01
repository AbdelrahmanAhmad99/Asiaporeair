using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Booking
{
    // DTO specifically for updating the payment status of a booking.
    public class UpdateBookingStatusDto
    {
        [Required]
        [StringLength(20)]
        public string NewStatus { get; set; } = string.Empty; // e.g., Confirmed, Cancelled
        public string? Reason { get; set; } // Optional reason for cancellation
    }
}