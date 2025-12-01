using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Flight
{
    // DTO for the public "Flight Status" check feature.
    public class FlightStatusRequestDto
    {
        [Required]
        [StringLength(10)]
        public string FlightNumber { get; set; } = string.Empty; // e.g., "SQ318"

        [Required]
        public DateTime FlightDate { get; set; }
    }
}