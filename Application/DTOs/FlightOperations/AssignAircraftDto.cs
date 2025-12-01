using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.FlightOperations
{
    // DTO for assigning a specific aircraft tail number to a flight instance.
    public class AssignAircraftDto
    {
        [Required]
        [StringLength(10)]
        public string TailNumber { get; set; } = string.Empty;
    }
}