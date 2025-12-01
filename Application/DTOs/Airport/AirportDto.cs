using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Airport
{
    /// <summary>
    /// Data Transfer Object representing an airport with basic country info.
    /// </summary>
    public class AirportDto
    {
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string IataCode { get; set; } = string.Empty;

        [Required]
        [StringLength(4, MinimumLength = 4)]
        public string IcaoCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string CountryIsoCode { get; set; } = string.Empty;  

        public string CountryName { get; set; } = string.Empty; 

        [Required]
        public decimal Latitude { get; set; }

        [Required]
        public decimal Longitude { get; set; }

        public int? Altitude { get; set; }
    }
}