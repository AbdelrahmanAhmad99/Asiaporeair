using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.AircraftType
{
    // Data Transfer Object for representing Aircraft Type details
    public class AircraftTypeDto
    {
        public int TypeId { get; set; } // Primary Key

        [Required]
        [StringLength(50)]
        public string Model { get; set; } = string.Empty; // e.g., "777-300ER", "A380-800"

        [Required]
        [StringLength(50)]
        public string Manufacturer { get; set; } = string.Empty; // e.g., "Boeing", "Airbus"

        public int? RangeKm { get; set; } // Maximum range in kilometers

        public int? MaxSeats { get; set; } // Maximum seating capacity

        public decimal? CargoCapacity { get; set; } // Cargo capacity (e.g., in cubic meters or kg)

        public int? CruisingVelocity { get; set; } // Typical cruising speed in km/h
    }
}