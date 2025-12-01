using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// Data Transfer Object for Country seeding process.
    /// Maps directly to the JSON structure and corresponds to the Country entity properties.
    /// </summary>
    public class CountrySeedDto
    {
        [JsonPropertyName("IsoCode")]
        public string IsoCode { get; set; } = string.Empty;

        [JsonPropertyName("Name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("Continent")]
        public string Continent { get; set; } = string.Empty;
    }
} 