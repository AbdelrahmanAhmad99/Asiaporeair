using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Airport
{
    /// <summary>
    /// Data Transfer Object for updating an existing airport.
    /// IATA and ICAO codes are typically not updatable.
    /// </summary>
    public class UpdateAirportDto
    {
        [Required(ErrorMessage = "Airport name is required.")]
        [StringLength(100, ErrorMessage = "Airport name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required.")]
        [StringLength(100, ErrorMessage = "City name cannot exceed 100 characters.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country ISO Code is required.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Country ISO Code must be exactly 3 characters.")]
        [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Country ISO Code must be 3 uppercase letters.")]
        public string CountryIsoCode { get; set; } = string.Empty; // Allow changing country if needed

        [Required(ErrorMessage = "Latitude is required.")]
        [Range(-90.0, 90.0, ErrorMessage = "Latitude must be between -90 and 90.")]
        public decimal Latitude { get; set; }

        [Required(ErrorMessage = "Longitude is required.")]
        [Range(-180.0, 180.0, ErrorMessage = "Longitude must be between -180 and 180.")]
        public decimal Longitude { get; set; }

        [Range(-1000, 30000, ErrorMessage = "Altitude seems out of reasonable range.")]
        public int? Altitude { get; set; }
    }
}