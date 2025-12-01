using System;
using System.Collections.Generic;
using Application.DTOs.Passenger;
using Application.DTOs.AncillaryProduct;  
using Application.DTOs.Payment;          

namespace Application.DTOs.Booking
{
    // DTO for displaying comprehensive booking details (e.g., "Manage Booking" page).
    public class BookingDetailDto
    {
        // Basic Info
        public int BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty;
        public DateTime BookingTime { get; set; }
        public decimal TotalPrice { get; set; }
        public string PaymentStatus { get; set; } = string.Empty;
        public bool IsCancelled => PaymentStatus == "Cancelled"; // Helper

        // User Info
        public int UserId { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;

        // Flight Info
        public int FlightInstanceId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string AirlineName { get; set; } = string.Empty;
        public string AircraftModel { get; set; } = string.Empty;
        public DateTime FlightDepartureTime { get; set; }
        public DateTime FlightArrivalTime { get; set; }
        public string OriginAirportCode { get; set; } = string.Empty;
        public string OriginAirportName { get; set; } = string.Empty;
        public string DestinationAirportCode { get; set; } = string.Empty;
        public string DestinationAirportName { get; set; } = string.Empty;
        public string FareBasisCode { get; set; } = string.Empty;
        public string FareDescription { get; set; } = string.Empty;

        // Passengers & Seats
        public List<BookingPassengerDetailDto> Passengers { get; set; } = new List<BookingPassengerDetailDto>();

        // Ancillaries
        public List<AncillarySaleDto> AncillarySales { get; set; } = new List<AncillarySaleDto>();

        // Payments
        public List<PaymentDto> Payments { get; set; } = new List<PaymentDto>();
    }
}