using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Route
{
    public class CreateRouteDto
    {
        [Required(ErrorMessage = "Origin Airport IATA Code is required.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "IATA Code must be exactly 3 characters.")]
        public string OriginAirportIataCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Destination Airport IATA Code is required.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "IATA Code must be exactly 3 characters.")]
        public string DestinationAirportIataCode { get; set; } = string.Empty;

        [Range(1, 40000, ErrorMessage = "Distance must be a positive value (up to 40,000 km).")]
        public int? DistanceKm { get; set; }
    }
}