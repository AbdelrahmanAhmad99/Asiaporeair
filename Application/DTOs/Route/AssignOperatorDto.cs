using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Route
{
    public class AssignOperatorDto
    {
        [Required(ErrorMessage = "Route ID is required.")]
        public int RouteId { get; set; }

        [Required(ErrorMessage = "Airline IATA Code is required.")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "IATA Code must be 2 characters.")]
        public string AirlineIataCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Codeshare status is required.")]
        public bool IsCodeshare { get; set; }
    }
}
