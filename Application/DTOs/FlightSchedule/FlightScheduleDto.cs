using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.FlightSchedule
{
    // DTO for displaying a planned flight schedule
    public class FlightScheduleDto
    {
        public int ScheduleId { get; set; }

        [Required]
        public string FlightNo { get; set; } = string.Empty;

        public int RouteId { get; set; }
        public string RouteName { get; set; } = string.Empty; // e.g., SIN - LHR

        public string AirlineIataCode { get; set; } = string.Empty;
        public string AircraftTypeModel { get; set; } = string.Empty;

        public DateTime DepartureTimeScheduled { get; set; }
        public DateTime ArrivalTimeScheduled { get; set; }

        public byte? DaysOfWeek { get; set; } // Bitmask for operating days
    }
}