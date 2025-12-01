using System;
using Domain.Enums;

namespace Application.DTOs.Ticket
{
    // DTO for displaying detailed ticket information (e.g., printable e-ticket view).
    public class TicketDetailDto
    {
        // Ticket Info
        public int TicketId { get; set; }
        public string TicketCode { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public string Status { get; set; } = TicketStatus.Issued.ToString();

        // Booking Info
        public int BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;

        // Passenger Info
        public int PassengerId { get; set; }
        public string PassengerFirstName { get; set; } = string.Empty;
        public string PassengerLastName { get; set; } = string.Empty;
        public DateTime? PassengerDateOfBirth { get; set; }
        public string? PassengerPassportNumber { get; set; }
        public string? FrequentFlyerNumber { get; set; }

        // Flight Info
        public int FlightInstanceId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string AirlineName { get; set; } = string.Empty;
        public DateTime FlightDepartureTime { get; set; }
        public DateTime FlightArrivalTime { get; set; }
        public string OriginAirportCode { get; set; } = string.Empty;
        public string OriginAirportName { get; set; } = string.Empty;
        public string DestinationAirportCode { get; set; } = string.Empty;
        public string DestinationAirportName { get; set; } = string.Empty;

        // Seat & Cabin
        public string? SeatId { get; set; }
        public string SeatNumber { get; set; } = "N/A";
        public string CabinClassName { get; set; } = string.Empty;

        // Fare Info
        public string FareBasisCode { get; set; } = string.Empty;
    }
}