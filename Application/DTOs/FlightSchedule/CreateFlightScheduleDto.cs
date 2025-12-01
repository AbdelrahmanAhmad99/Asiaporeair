using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.FlightSchedule
{
    // DTO for creating a new flight schedule
    public class CreateFlightScheduleDto
    {
        [Required(ErrorMessage = "Flight number is required.")]
        [StringLength(10)]
        public string FlightNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Route ID is required.")]
        public int RouteId { get; set; }

        [Required(ErrorMessage = "Airline IATA Code is required.")]
        [StringLength(2, MinimumLength = 2)]
        public string AirlineIataCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Aircraft Type ID is required.")]
        public int AircraftTypeId { get; set; }

        [Required(ErrorMessage = "Scheduled Departure Time is required.")]
        public DateTime DepartureTimeScheduled { get; set; }

        [Required(ErrorMessage = "Scheduled Arrival Time is required.")]
        public DateTime ArrivalTimeScheduled { get; set; }

        [Range(1, 127, ErrorMessage = "Days of week must be a valid bitmask value (1-127).")]
        public byte? DaysOfWeek { get; set; } // Bitmask
    }
}
