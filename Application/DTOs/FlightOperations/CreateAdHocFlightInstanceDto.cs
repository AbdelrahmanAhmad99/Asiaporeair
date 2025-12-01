using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.FlightOperations
{
    // DTO for creating a single, unscheduled flight (e.g., charter, ferry flight).
    public class CreateAdHocFlightInstanceDto
    {
        [Required]
        public int RouteId { get; set; }

        [Required]
        public string AirlineIataCode { get; set; } = string.Empty;

        [Required]
        public string FlightNo { get; set; } = string.Empty; // e.g., "SQ8001" (Charter)

        [Required]
        public DateTime ScheduledDeparture { get; set; }

        [Required]
        public DateTime ScheduledArrival { get; set; }

        [Required]
        public string AircraftTailNumber { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Scheduled";
    }
}