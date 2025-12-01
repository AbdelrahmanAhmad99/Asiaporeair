using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Airline
{
    // Data Transfer Object for creating a new Airline
    public class CreateAirlineDto
    {
        [Required(ErrorMessage = "IATA Code is required.")]
        [StringLength(2, MinimumLength = 2, ErrorMessage = "IATA Code must be exactly 2 characters.")]
        [RegularExpression(@"^[A-Z0-9]{2}$", ErrorMessage = "IATA Code must be 2 uppercase letters or digits.")]
        public string IataCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Airline name is required.")]
        [StringLength(100, ErrorMessage = "Airline name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Callsign is required.")]
        [StringLength(50, ErrorMessage = "Callsign cannot exceed 50 characters.")]
        public string Callsign { get; set; } = string.Empty;

        [Required(ErrorMessage = "Operating region is required.")]
        [StringLength(50, ErrorMessage = "Operating region cannot exceed 50 characters.")]
        public string OperatingRegion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Base Airport IATA Code is required.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Base Airport IATA Code must be exactly 3 characters.")]
        [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Base Airport IATA Code must be 3 uppercase letters.")]
        public string BaseAirportIataCode { get; set; } = string.Empty;
    }
}