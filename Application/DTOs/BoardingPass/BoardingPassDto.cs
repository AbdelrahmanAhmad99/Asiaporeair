using System;

namespace Application.DTOs.BoardingPass
{
    // DTO for displaying boarding pass information.
    public class BoardingPassDto
    {
        public int PassId { get; set; }

        // Passenger Info
        public int PassengerId { get; set; }
        public string PassengerName { get; set; } = string.Empty;
        public string? FrequentFlyerNumber { get; set; }

        // Flight Info
        public int FlightInstanceId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string OriginAirportCode { get; set; } = string.Empty;
        public string DestinationAirportCode { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }

        // Boarding Info
        public string SeatNumber { get; set; } = string.Empty;
        public string CabinClass { get; set; } = string.Empty;
        public DateTime BoardingTime { get; set; }
        //public string? Gate { get; set; } // Gate info might come from Flight Instance operational data
        public int SequenceNumber { get; set; } // Boarding sequence number
        public bool? PrecheckStatus { get; set; } // e.g., TSA PreCheck

        // Booking Info
        public string BookingReference { get; set; } = string.Empty;
        public string TicketCode { get; set; } = string.Empty; // Associated e-ticket code
    }
}