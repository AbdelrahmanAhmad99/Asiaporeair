using System;
using Application.DTOs.AncillaryProduct;  
using Application.DTOs.Payment;
namespace Application.DTOs.Booking
{
    // DTO for searching/filtering bookings (Admin/Support view).
    public class BookingFilterDto
    {
        public string? BookingReference { get; set; }
        public int? FlightInstanceId { get; set; }
        public string? PassengerNameContains { get; set; }
        public string? PassengerPassport { get; set; }
        public string? PaymentStatus { get; set; } // e.g., Pending, Confirmed, Cancelled
        public DateTime? BookingDateFrom { get; set; }
        public DateTime? BookingDateTo { get; set; }
        public int? UserId { get; set; } // Filter by the User who made the booking
        public bool IncludeDeleted { get; set; } = false;
    }
}