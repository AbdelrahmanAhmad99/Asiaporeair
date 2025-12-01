using System;
using System.Text.Json.Serialization;

namespace Infrastructure.Data.DataSeeding.SeedDtos
{
    /// <summary>
    /// Data Transfer Object for deserializing FlightSchedule data from JSON seed file.
    /// Property names are set via JsonPropertyName attribute to match the JSON keys.
    /// </summary>
    public class FlightScheduleSeedDto
    {
        [JsonPropertyName("schedule_id")]
        public int ScheduleId { get; set; }

        [JsonPropertyName("flight_no")]
        public string FlightNo { get; set; } = string.Empty;

        [JsonPropertyName("route_id")]
        public int RouteId { get; set; }

        [JsonPropertyName("airline_id")]
        public string AirlineId { get; set; } = string.Empty;

        [JsonPropertyName("aircraft_type_id")]
        public int AircraftTypeId { get; set; }

        [JsonPropertyName("departure_time_scheduled")]
        public DateTime DepartureTimeScheduled { get; set; }

        [JsonPropertyName("arrival_time_scheduled")]
        public DateTime ArrivalTimeScheduled { get; set; }

        // Nullable byte for Days of Week (e.g., 1 for Monday, 64 for Sunday, bitwise flag)
        [JsonPropertyName("days_of_week")]
        public byte? DaysOfWeek { get; set; }

        [JsonPropertyName("is_deleted")]
        public bool IsDeleted { get; set; }
    }
}