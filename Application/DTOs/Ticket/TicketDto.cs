using System;
using Domain.Enums; // Assuming TicketStatus enum is here

namespace Application.DTOs.Ticket
{
    // DTO for displaying summary ticket information (e.g., in lists).
    public class TicketDto
    {
        public int TicketId { get; set; }
        public string TicketCode { get; set; } = string.Empty; // Unique e-ticket number/code
        public int BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty; // Added for context
        public int PassengerId { get; set; }
        public string PassengerName { get; set; } = string.Empty; // Populated by service
        public string FlightNumber { get; set; } = string.Empty; // Populated by service
        public DateTime FlightDepartureTime { get; set; } // Added for context
        public string SeatNumber { get; set; } = string.Empty; // Populated by service
        public DateTime IssueDate { get; set; }
        public string Status { get; set; } = TicketStatus.Issued.ToString(); // Status as string
    }
}