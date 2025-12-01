using System.Text.Json.Serialization;

namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// DTO for FlightLegDef entity seeding, used to deserialize data from a JSON file.
    /// Properties are mapped to the snake_case column names or the entity properties.
    /// </summary>
    public class FlightLegDefSeedDto
    { 
        [JsonPropertyName("leg_def_id")]
        public int LegDefId { get; set; }
         
        [JsonPropertyName("schedule_fk")]
        public int ScheduleId { get; set; }
         
        [JsonPropertyName("segment_number")]
        public int SegmentNumber { get; set; }
         
        [JsonPropertyName("departure_airport_fk")]
        public string DepartureAirportId { get; set; }
         
        [JsonPropertyName("arrival_airport_fk")]
        public string ArrivalAirportId { get; set; }
         
        [JsonPropertyName("IsDeleted")]
        public bool IsDeleted { get; set; }
    }
}