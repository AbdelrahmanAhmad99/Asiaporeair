using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.AircraftType
{
    // Data Transfer Object for creating a new Aircraft Type
    public class CreateAircraftTypeDto
    {
        [Required(ErrorMessage = "Model name is required.")]
        [StringLength(50, ErrorMessage = "Model name cannot exceed 50 characters.")]
        public string Model { get; set; } = string.Empty;

        [Required(ErrorMessage = "Manufacturer name is required.")]
        [StringLength(50, ErrorMessage = "Manufacturer name cannot exceed 50 characters.")]
        public string Manufacturer { get; set; } = string.Empty;

        [Range(0, 30000, ErrorMessage = "Range must be a positive value (up to 30000 km).")]
        public int? RangeKm { get; set; }

        [Range(1, 1000, ErrorMessage = "Maximum seats must be between 1 and 1000.")]
        public int? MaxSeats { get; set; }

        [Range(0.01, 10000.00, ErrorMessage = "Cargo capacity must be a positive value.")]
        public decimal? CargoCapacity { get; set; }

        [Range(100, 1200, ErrorMessage = "Cruising velocity must be between 100 and 1200 km/h.")]
        public int? CruisingVelocity { get; set; }
    }
}