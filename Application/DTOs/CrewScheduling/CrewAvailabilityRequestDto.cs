using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.CrewScheduling
{
    // DTO to find available crew for a potential assignment.
    public class CrewAvailabilityRequestDto
    {
        [Required]
        public DateTime FlightDepartureTime { get; set; }

        [Required]
        public DateTime FlightArrivalTime { get; set; }

        public string? RequiredPosition { get; set; } // "Pilot" or "Attendant"
        public string? RequiredBaseAirportIata { get; set; } // Optional: Prefer crew from a specific base
        public int? RequiredAircraftTypeId { get; set; } // Required for Pilots
    }
}