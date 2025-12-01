using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// Data Transfer Object (DTO) for professional deserialization of Airport seed data from JSON.
    /// This structure mirrors the essential properties of the Airport entity, ensuring a clean mapping process.
    /// </summary>
    public class AirportSeedDto
    {
        // Primary Key - IATA Code (3-letter identifier)
        [JsonPropertyName("iata_code")]
        public string IataCode { get; set; }

        // ICAO Code (4-letter identifier)
        [JsonPropertyName("icao_code")]
        public string IcaoCode { get; set; }

        // Full name of the airport
        [JsonPropertyName("name")]
        public string Name { get; set; }

        // The city where the airport is located
        [JsonPropertyName("city")]
        public string City { get; set; }

        // Foreign Key to the Country entity (ISO 3166-1 alpha-3 code)
        [JsonPropertyName("country_fk")]
        public string CountryId { get; set; }

        // Geographical latitude (Required)
        [JsonPropertyName("latitude")]
        public decimal Latitude { get; set; }

        // Geographical longitude (Required)
        [JsonPropertyName("longitude")]
        public decimal Longitude { get; set; }

        // Altitude above sea level (Optional/Nullable)
        [JsonPropertyName("altitude")]
        public int? Altitude { get; set; }

        // Soft delete flag (default to false during seeding)
        [JsonPropertyName("IsDeleted")]
        public bool IsDeleted { get; set; } = false;
    }
}