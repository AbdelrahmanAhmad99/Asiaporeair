using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Aircraft
{
    // DTO for adding a cabin class to a configuration
    public class CreateCabinClassDto
    {
        [Required(ErrorMessage = "Configuration ID is required.")]
        public int ConfigId { get; set; }

        [Required(ErrorMessage = "Cabin Class Name is required.")]
        [StringLength(20, ErrorMessage = "Cabin Class Name cannot exceed 20 characters.")]
        public string Name { get; set; } = string.Empty;
    }
}