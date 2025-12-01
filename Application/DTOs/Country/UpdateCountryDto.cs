using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Country
{
    /// <summary>
    /// Data Transfer Object for updating an existing country.
    /// ISO Code is usually not updatable.
    /// </summary>
    public class UpdateCountryDto
    {
        [Required(ErrorMessage = "Country name is required.")]
        [StringLength(100, ErrorMessage = "Country name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Continent is required.")]
        [StringLength(50, ErrorMessage = "Continent name cannot exceed 50 characters.")]
        public string Continent { get; set; } = string.Empty;
    }
}