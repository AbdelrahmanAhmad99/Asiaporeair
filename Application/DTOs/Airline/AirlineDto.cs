using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Airline
{
    // Data Transfer Object for Airline information
    public class AirlineDto
    {
        [Required]
        [StringLength(2, MinimumLength = 2)]
        public string IataCode { get; set; } = string.Empty; 

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;  

        [Required]
        [StringLength(50)]
        public string Callsign { get; set; } = string.Empty; 

        [Required]
        [StringLength(50)]
        public string OperatingRegion { get; set; } = string.Empty;  

        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string BaseAirportIataCode { get; set; } = string.Empty;  

        public string BaseAirportName { get; set; } = string.Empty;  
    }
}