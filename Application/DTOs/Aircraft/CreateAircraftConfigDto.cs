using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Aircraft
{
    // DTO for creating a new aircraft configuration
    public class CreateAircraftConfigDto
    {
        [Required(ErrorMessage = "Configuration Name is required.")]
        [StringLength(50, ErrorMessage = "Configuration Name cannot exceed 50 characters.")]
        public string ConfigurationName { get; set; } = string.Empty;
          
        [Required(ErrorMessage = "Total Seats Count is required.")]
        [Range(1, 1000, ErrorMessage = "Total seats must be between 1 and 1000.")]
        public int TotalSeatsCount { get; set; }
        
    }
}