using System.Text.Json.Serialization;

namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// DTO for deserializing FlightCrew data from a JSON file.
    /// This structure mirrors the required properties for a FlightCrew entity seed.
    /// </summary>
    public class FlightCrewSeedDto
    { 
        [JsonPropertyName("flight_instance_fk")]
        public int FlightInstanceId { get; set; }
         
        [JsonPropertyName("crew_member_fk")]
        public int CrewMemberId { get; set; }
         
        [JsonPropertyName("role")]
        public string Role { get; set; } = string.Empty;
         
        [JsonPropertyName("IsDeleted")]
        public bool IsDeleted { get; set; } = false;
    }
}