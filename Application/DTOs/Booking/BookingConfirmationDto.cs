using System.Collections.Generic;

namespace Application.DTOs.Booking
{
    // DTO returned after successful booking and payment confirmation.
    public class BookingConfirmationDto
    {
        public int BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public string ConfirmationMessage { get; set; } = string.Empty;
        // Include ticket numbers/IDs generated for the passengers.
        public List<TicketSummaryDto> GeneratedTickets { get; set; } = new List<TicketSummaryDto>();
    }

    // Helper for showing ticket info in confirmation.
    public class TicketSummaryDto
    {
        public int TicketId { get; set; }
        public string TicketCode { get; set; } = string.Empty;
        public int PassengerId { get; set; }
        public string PassengerName { get; set; } = string.Empty;
    }
}