using System;
using System.Collections.Generic;
using Application.DTOs.Passenger; // Keep reference to PassengerDto

namespace Application.DTOs.Booking
{
    // Standard DTO for displaying booking summary information.
    public class BookingDto
    {
        public int BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public DateTime BookingTime { get; set; }
        public decimal TotalPrice { get; set; }
        public string PaymentStatus { get; set; } = string.Empty; // e.g., Pending, Confirmed, Cancelled
        public string FlightNumber { get; set; } = string.Empty;
        public DateTime FlightDepartureTime { get; set; }
        public string OriginAirportCode { get; set; } = string.Empty;
        public string DestinationAirportCode { get; set; } = string.Empty;
        public string FareBasisCode { get; set; } = string.Empty;
        public List<PassengerDto> Passengers { get; set; } = new List<PassengerDto>(); // Use PassengerDto
    }
}