using System;
using System.Collections.Generic;

namespace Application.DTOs.Flight
{
    // Represents a complete journey option (one-way or round-trip) shown to the user.
    public class FlightItineraryDto
    {
        // A unique ID for this specific itinerary option
        public string ItineraryId { get; set; } = string.Empty;

        // List of segments for the outbound journey (e.g., SIN-DXB, DXB-LHR)
        public List<FlightSegmentDto> OutboundSegments { get; set; } = new List<FlightSegmentDto>();

        // List of segments for the inbound journey (for round-trip)
        public List<FlightSegmentDto>? InboundSegments { get; set; }

        // The different pricing options available for this *entire* itinerary
        public List<FlightFareOptionDto> FareOptions { get; set; } = new List<FlightFareOptionDto>();

        public int TotalDurationMinutes { get; set; }
        public int NumberOfStops { get; set; }
    }
}