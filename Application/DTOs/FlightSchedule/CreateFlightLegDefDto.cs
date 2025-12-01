using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.FlightSchedule
{
    /// <summary>
    /// DTO for creating a new flight leg (segment) definition 
    /// associated with an existing Flight Schedule.
    /// </summary>
    public class CreateFlightLegDefDto
    {
        [Required(ErrorMessage = "Segment number is required.")]
        [Range(1, 20, ErrorMessage = "Segment number must be between 1 and 20.")]
        public int SegmentNumber { get; set; }

        [Required(ErrorMessage = "Departure airport IATA code is required.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Departure IATA code must be 3 characters.")]
        public string DepartureAirportIataCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Arrival airport IATA code is required.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Arrival IATA code must be 3 characters.")]
        public string ArrivalAirportIataCode { get; set; } = string.Empty;
    }
}