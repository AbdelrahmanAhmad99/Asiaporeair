using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.FareBasisCode
{
    // DTO for creating a new Fare Basis Code
    public class CreateFareBasisCodeDto
    {
        [Required(ErrorMessage = "Fare code is required.")]
        [StringLength(10, MinimumLength = 1, ErrorMessage = "Fare code must be between 1 and 10 characters.")]
        [RegularExpression(@"^[A-Z0-9/]+$", ErrorMessage = "Fare code must be uppercase alphanumeric or contain '/'.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rules are required.")]
        public string Rules { get; set; } = string.Empty;
    }
}