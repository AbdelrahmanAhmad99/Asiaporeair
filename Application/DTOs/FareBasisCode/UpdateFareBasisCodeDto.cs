using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.FareBasisCode
{ 
    public class UpdateFareBasisCodeDto
    {
        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rules are required.")]
        public string Rules { get; set; } = string.Empty;
    }
}