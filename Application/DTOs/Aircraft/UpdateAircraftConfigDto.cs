using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Aircraft
{
    // DTO for updating an existing aircraft configuration
    public class UpdateAircraftConfigDto
    {
        [Required(ErrorMessage = "Configuration Name is required.")]
        [StringLength(50)]
        public string ConfigurationName { get; set; }

        // (THE FIX) Add the field you wanted
        [Required]
        [Range(1, 1000, ErrorMessage = "Total seats must be between 1 and 1000.")]
        public int TotalSeatsCount { get; set; }
    }
}