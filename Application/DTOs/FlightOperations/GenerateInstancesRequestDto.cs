using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.FlightOperations
{
    // DTO for requesting the batch generation of flight instances from schedules.
    public class GenerateInstancesRequestDto
    {
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        // Optional: Only generate for a specific airline. If null, generate for all.
        public string? AirlineIataCode { get; set; }
    }
}