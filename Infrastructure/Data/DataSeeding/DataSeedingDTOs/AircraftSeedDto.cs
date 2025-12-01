using System;
using System.Text.Json.Serialization;

namespace Infrastructure.Data.DataSeeding.SeedDtos
{
    /// <summary>
    /// Data Transfer Object (DTO) for deserializing Aircraft seed data from JSON file.
    /// Maps the JSON property names (database column names) to the C# entity properties.
    /// </summary>
    public class AircraftSeedDto
    { 
        [JsonPropertyName("tail_number")]
        public string TailNumber { get; set; }
         
        [JsonPropertyName("airline_fk")]
        public string AirlineId { get; set; }
         
        [JsonPropertyName("aircraft_type_fk")]
        public int AircraftTypeId { get; set; }
         
        [JsonPropertyName("total_flight_hours")]
        public int TotalFlightHours { get; set; }
         
        [JsonPropertyName("acquisition_date")]
        public DateTime AcquisitionDate { get; set; }

        // Current operational status (e.g., 'Active', 'Maintenance', 'Storage').
        [JsonPropertyName("status")]
        public string Status { get; set; }
         
        [JsonPropertyName("IsDeleted")]
        public bool IsDeleted { get; set; } = false;
    }
}