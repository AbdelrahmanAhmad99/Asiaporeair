using System;

namespace Application.DTOs.FlightOperations
{
    // A lightweight DTO for Flight Information Display Systems (FIDS) or list views.
    public class FlightInstanceBriefDto
    {
        public int InstanceId { get; set; }
        public string FlightNo { get; set; } = string.Empty;
        public string AirlineName { get; set; } = string.Empty;
        public string AirlineIataCode { get; set; } = string.Empty;

        // Route
        public string OriginCity { get; set; } = string.Empty;
        public string OriginIataCode { get; set; } = string.Empty;
        public string DestinationCity { get; set; } = string.Empty;
        public string DestinationIataCode { get; set; } = string.Empty;

        // Times
        public DateTime ScheduledTime { get; set; } // Shows Departure or Arrival based on context
        //public DateTime? EstimatedTime { get; set; } // Shows Estimated Departure/Arrival

        // Status
        //public string Status { get; set; } = string.Empty;
        //public string? Gate { get; set; }
        //public string? BaggageCarousel { get; set; }
    }
}