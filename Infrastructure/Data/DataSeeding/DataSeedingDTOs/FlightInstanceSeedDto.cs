 
using System.Text.Json.Serialization;

namespace Infrastructure.Data.DataSeeding.DataSeedingDTOs
{
    /// <summary>
    /// Data Transfer Object for deserializing FlightInstance records from the JSON seed file.
    /// This structure mirrors the FlightInstance entity but excludes the primary key (InstanceId) 
    /// as it is an IDENTITY column managed by the database.
    /// </summary>
    public class FlightInstanceSeedDto
    { 
        [JsonPropertyName("schedule_fk")]
        public int ScheduleId { get; set; }
         
        [JsonPropertyName("aircraft_fk")]
        public string AircraftId { get; set; }
         
        [JsonPropertyName("scheduled_dep_ts")]
        public DateTime ScheduledDeparture { get; set; }
         
        [JsonPropertyName("actual_dep_ts")]
        public DateTime? ActualDeparture { get; set; }
         
        [JsonPropertyName("scheduled_arr_ts")]
        public DateTime ScheduledArrival { get; set; }
         
        [JsonPropertyName("actual_arr_ts")]
        public DateTime? ActualArrival { get; set; }
         
        [JsonPropertyName("status")]
        public string Status { get; set; } = "Scheduled";
         
        [JsonPropertyName("IsDeleted")]
        public bool IsDeleted { get; set; } = false;
    }
}