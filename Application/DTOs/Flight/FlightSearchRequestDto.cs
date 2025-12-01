using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Flight
{
    // DTO for a comprehensive flight search, supporting one-way, round-trip, and multi-city.
    public class FlightSearchRequestDto
    {
        [Required]
        public string SearchType { get; set; } = "OneWay"; // "OneWay", "RoundTrip", "MultiCity"

        [Required]
        public PassengerCountDto Passengers { get; set; } = new PassengerCountDto();

        [Required]
        [MinLength(1)]
        public List<SearchSegmentDto> Segments { get; set; } = new List<SearchSegmentDto>();

        // e.g., "Economy", "Business", "First". This is a preference.
        public string CabinClassPreference { get; set; } = "Economy";

        // Allows searching for different fare types (e.g., include flexible fares)
        public bool IncludeFlexibleFares { get; set; } = false;
    }

    // Represents one leg of the search (e.g., SIN to LHR on a specific date)
    public class SearchSegmentDto
    {
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string OriginIataCode { get; set; } = string.Empty;

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string DestinationIataCode { get; set; } = string.Empty;

        [Required]
        public DateTime DepartureDate { get; set; }
    }

    // Represents the passenger mix
    public class PassengerCountDto
    {
        [Range(0, 9)]
        public int Adults { get; set; } = 1;

        [Range(0, 9)]
        public int Children { get; set; } = 0; // Age 2-11

        [Range(0, 9)]
        public int Infants { get; set; } = 0; // Under 2

        // Helper property
        public int TotalPassengers => Adults + Children; // Infants often don't occupy a seat
    }
}