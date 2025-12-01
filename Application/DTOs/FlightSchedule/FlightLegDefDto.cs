using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.FlightSchedule
{
    /// <summary>
    /// DTO for displaying a defined flight leg (segment) of a Flight Schedule.
    /// </summary>
    public class FlightLegDefDto
    {
        public int LegDefId { get; set; }
        public int ScheduleId { get; set; }
        public int SegmentNumber { get; set; }

        [Required]
        [StringLength(3)]
        public string DepartureAirportIataCode { get; set; } = string.Empty;

        // Added for UI display friendliness, populated by the service
        public string DepartureAirportName { get; set; } = string.Empty;

        [Required]
        [StringLength(3)]
        public string ArrivalAirportIataCode { get; set; } = string.Empty;

        // Added for UI display friendliness, populated by the service
        public string ArrivalAirportName { get; set; } = string.Empty;
    }
}