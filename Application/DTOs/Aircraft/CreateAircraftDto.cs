using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Aircraft
{
    // DTO for registering a new aircraft in the fleet
    public class CreateAircraftDto
    {
        [Required(ErrorMessage = "Tail Number is required.")]
        [StringLength(10, ErrorMessage = "Tail Number cannot exceed 10 characters.")]
        [RegularExpression(@"^[A-Z0-9-]+$", ErrorMessage = "Tail Number must be alphanumeric with hyphens.")]
        public string TailNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Airline IATA Code is required.")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Airline IATA Code must be 2 characters.")]
        public string AirlineIataCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Aircraft Type ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Aircraft Type ID.")]
        public int AircraftTypeId { get; set; }

        public DateTime? AcquisitionDate { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(20, ErrorMessage = "Status cannot exceed 20 characters.")]
        public string Status { get; set; } = "Grounded"; // Default status
    }
}