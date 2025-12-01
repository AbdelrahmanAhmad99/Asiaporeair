using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Country
{
    /// <summary>
    /// Data Transfer Object for creating a new country.
    /// </summary>
    public class CreateCountryDto
    {
        [Required(ErrorMessage = "ISO Code is required.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "ISO Code must be exactly 3 characters.")]
        [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "ISO Code must be 3 uppercase letters.")]
        public string IsoCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country name is required.")]
        [StringLength(100, ErrorMessage = "Country name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Continent is required.")]
        [StringLength(50, ErrorMessage = "Continent name cannot exceed 50 characters.")]
        public string Continent { get; set; } = string.Empty;
    }
}