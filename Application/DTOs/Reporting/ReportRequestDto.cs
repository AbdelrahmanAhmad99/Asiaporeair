using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Reporting
{
    // A standard DTO for requesting reports based on a date range and optional filters.
    public class ReportRequestDto
    {
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        // Optional filter for airport-specific reports (e.g., performance at SIN)
        public string? AirportIataCode { get; set; }

        // Optional filter for airline-specific reports (e.g., sales for SQ)
        public string? AirlineIataCode { get; set; }

        // Optional filter for crew reports
        //public string? CrewBaseIata { get; set; }
    }
}