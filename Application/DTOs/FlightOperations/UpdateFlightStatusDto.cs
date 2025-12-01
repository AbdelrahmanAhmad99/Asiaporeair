using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.FlightOperations
{
    // DTO for updating the operational status of a flight.
    public class UpdateFlightStatusDto
    {
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty; // e.g., OnTime, Delayed, Cancelled, Boarding

        public string? Reason { get; set; } // Optional reason for delay/cancellation
    }
}