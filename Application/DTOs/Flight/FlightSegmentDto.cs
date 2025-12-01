using System;

namespace Application.DTOs.Flight
{
    // Represents a single flight leg (e.g., SQ318 from SIN to LHR) within an itinerary.
    public class FlightSegmentDto
    {
        public int FlightInstanceId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;

        public string AirlineName { get; set; } = string.Empty;
        public string AirlineIataCode { get; set; } = string.Empty;

        public string AircraftModel { get; set; } = string.Empty;

        // Origin Details
        public string OriginAirportIata { get; set; } = string.Empty;
        public string OriginAirportName { get; set; } = string.Empty;
        public string OriginCity { get; set; } = string.Empty;
        public DateTime DepartureTime { get; set; } // Local time of origin

        // Destination Details
        public string DestinationAirportIata { get; set; } = string.Empty;
        public string DestinationAirportName { get; set; } = string.Empty;
        public string DestinationCity { get; set; } = string.Empty;
        public DateTime ArrivalTime { get; set; } // Local time of destination

        public int DurationMinutes { get; set; }
    }
}