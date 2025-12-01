using System;

namespace Application.DTOs.FlightOperations
{
    // DTO for advanced filtering of flight instances (Airport Management view).
    public class FlightInstanceFilterDto
    {
        public string? FlightNo { get; set; }
        public string? AirportIataCode { get; set; } // Can be Origin or Destination
        public string? Direction { get; set; } // "Departure" or "Arrival"
        public string? AirlineIataCode { get; set; }
        public string? Status { get; set; }
        public DateTime? Date { get; set; } // The specific date to check
        //public string? Gate { get; set; }
        public string? AircraftTailNumber { get; set; }
        public bool IncludeDeleted { get; set; } = false;
    }
}