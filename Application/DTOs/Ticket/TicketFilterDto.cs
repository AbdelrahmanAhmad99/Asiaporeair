using System;
using Domain.Enums;

namespace Application.DTOs.Ticket
{
    // DTO for searching/filtering tickets (Admin/Support).
    public class TicketFilterDto
    {
        public string? TicketCode { get; set; }
        public int? BookingId { get; set; }
        public string? BookingReference { get; set; }
        public int? FlightInstanceId { get; set; }
        public string? PassengerNameContains { get; set; }
        public string? PassengerPassport { get; set; }
        public string? Status { get; set; } // Use string representation of TicketStatus enum
        public DateTime? IssueDateFrom { get; set; }
        public DateTime? IssueDateTo { get; set; }
        public bool IncludeDeleted { get; set; } = false;
    }
}