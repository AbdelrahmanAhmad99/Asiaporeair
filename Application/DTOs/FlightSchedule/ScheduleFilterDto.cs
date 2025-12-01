using System;

namespace Application.DTOs.FlightSchedule
{
    // DTO for advanced filtering of flight schedules
    public class ScheduleFilterDto
    {
        public string? FlightNo { get; set; }
        public string? OriginIataCode { get; set; }
        public string? DestinationIataCode { get; set; }
        public DateTime? DepartureDate { get; set; }
        public string? AirlineIataCode { get; set; }
        public bool IncludeDeleted { get; set; } = false;
    }
}