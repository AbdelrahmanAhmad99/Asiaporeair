using System;
using System.Collections.Generic;

namespace Application.DTOs.Flight
{
    // DTO for the "Flight Details" modal/page (shows aircraft, amenities, etc.)
    // This is an enhanced version of the one from the user's file.
    public class FlightDetailsDto
    {
        public int FlightInstanceId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string AirlineName { get; set; } = string.Empty;
        public string AircraftModel { get; set; } = string.Empty;
        public string TailNumber { get; set; } = string.Empty;

        // Route & Time
        public string OriginAirportName { get; set; } = string.Empty;
        public string OriginIataCode { get; set; } = string.Empty;
        public DateTime ScheduledDepartureTime { get; set; }

        public string DestinationAirportName { get; set; } = string.Empty;
        public string DestinationIataCode { get; set; } = string.Empty;
        public DateTime ScheduledArrivalTime { get; set; }

        public int DurationMinutes { get; set; }
        public string Status { get; set; } = string.Empty;

        // Cabin and seat availability details
        public List<CabinClassAvailabilityDto> CabinClasses { get; set; } = new List<CabinClassAvailabilityDto>();

        // Placeholder for amenities (would come from AircraftType or config)
        public List<string> Amenities { get; set; } = new List<string> { "In-flight Entertainment", "Wi-Fi (Paid)", "Meals" };
    }
}