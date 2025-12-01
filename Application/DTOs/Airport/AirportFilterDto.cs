using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Airport
{
    /// <summary>
    /// Data Transfer Object for filtering airport searches. All properties are optional.
    /// </summary>
    public class AirportFilterDto
    {
        public string? NameContains { get; set; }
        public string? City { get; set; }
        public string? CountryIsoCode { get; set; }
        public string? Continent { get; set; } // Filter by continent via Country
        [Range(-90.0, 90.0, ErrorMessage = "MinLatitude must be between -90 and 90.")]
        public decimal? MinLatitude { get; set; }

        [Range(-90.0, 90.0, ErrorMessage = "MaxLatitude must be between -90 and 90.")]
        public decimal? MaxLatitude { get; set; }
        [Range(-180.0, 180.0, ErrorMessage = "MinLatitude must be between -90 and 90.")]
        public decimal? MinLongitude { get; set; }

        [Range(-180.0, 180.0, ErrorMessage = "MaxAltitude must be a realistic value.")]
        public decimal? MaxLongitude { get; set; }
        [Range(-500.0, 20000.0, ErrorMessage = "MinAltitude must be a realistic value.")]
        public decimal? MinAltitude { get; set; }

        [Range(-500.0, 20000.0, ErrorMessage = "MaxAltitude must be a realistic value.")]
        public decimal? MaxAltitude { get; set; }
        public bool IncludeDeleted { get; set; } = false; // Default to showing only active
    }
}