using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Country
{
    /// <summary>
    /// Data Transfer Object representing a country.
    /// </summary>
    public class CountryDto
    {
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string IsoCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Continent { get; set; } = string.Empty;
    }
}