using System.Text.Json.Serialization;

namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// Data Transfer Object for Route entity, used for professional deserialization 
    /// from the Route.json file, mapping JSON keys to C# properties.
    /// </summary>
    public class RouteSeedDto
    { 
        [JsonPropertyName("origin_airport_fk")]
        public string OriginAirportId { get; set; }
         
        [JsonPropertyName("destination_airport_fk")]
        public string DestinationAirportId { get; set; }
         
        [JsonPropertyName("distance_km")]
        public int? DistanceKm { get; set; }
         
        [JsonPropertyName("IsDeleted")]
        public bool IsDeleted { get; set; } = false;
    }
}