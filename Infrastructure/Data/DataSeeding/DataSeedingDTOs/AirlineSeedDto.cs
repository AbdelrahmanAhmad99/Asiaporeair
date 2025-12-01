using System.Text.Json.Serialization;

namespace Infrastructure.Data.DataSeeding.SeedDtos
{
    /// <summary>
    /// Data Transfer Object (DTO) for deserializing Airline seed data from JSON file.
    /// Maps the JSON property names (database column names) to the C# entity properties.
    /// </summary>
    public class AirlineSeedDto
    {
        // Maps to the Primary Key and IATA Code of the airline.
        [JsonPropertyName("iata_code")]
        public string IataCode { get; set; }

        // Maps to the full name of the airline.
        [JsonPropertyName("name")]
        public string Name { get; set; }

        // Maps to the unique callsign used in air traffic control.
        [JsonPropertyName("callsign")]
        public string Callsign { get; set; }

        // Maps to the primary operating region/hub focus of the airline.
        [JsonPropertyName("operating_region")]
        public string OperatingRegion { get; set; }

        // Foreign Key: Maps to the IATA code of the airline's base airport.
        // MUST match an iata_code from Airport.json.
        [JsonPropertyName("base_airport_fk")]
        public string BaseAirportId { get; set; }

        // Maps to the soft-delete flag, defaulting to false.
        [JsonPropertyName("IsDeleted")]
        public bool IsDeleted { get; set; } = false;
    }
}