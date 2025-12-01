using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Aircraft
{
    // DTO for updating an aircraft's core details (TailNumber is PK, not updatable)
    public class UpdateAircraftDto
    {
        [Required(ErrorMessage = "Airline IATA Code is required.")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "Airline IATA Code must be 2 characters.")]
        public string AirlineIataCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Aircraft Type ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Aircraft Type ID.")]
        public int AircraftTypeId { get; set; }

        public DateTime? AcquisitionDate { get; set; }
    }
}