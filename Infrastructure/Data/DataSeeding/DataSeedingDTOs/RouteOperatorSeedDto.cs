using System.Text.Json.Serialization;

namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// DTO for deserializing RouteOperator data from JSON.
    /// Matches the RouteOperator entity properties with JsonPropertyName attributes.
    /// </summary>
    public class RouteOperatorSeedDto
    { 
        [JsonPropertyName("route_id")]
        public int RouteId { get; set; }
         
        [JsonPropertyName("airline_id")]
        public string AirlineId { get; set; }
         
        [JsonPropertyName("codeshare_status")]
        public bool? CodeshareStatus { get; set; }
         
        [JsonPropertyName("IsDeleted")]
        public bool IsDeleted { get; set; } = false;
    }
}