using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks; 
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Flight
{
    public class FlightSearchDto
    {
        [Required]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Origin IATA code must be 3 characters.")]
        public string OriginIataCode { get; set; }

        [Required]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Destination IATA code must be 3 characters.")]
        public string DestinationIataCode { get; set; }

        [Required]
        public DateTime DepartureDate { get; set; }

        public DateTime? ReturnDate { get; set; } // Nullable for one-way trips

        [Range(1, 9, ErrorMessage = "Number of adults must be between 1 and 9.")]
        public int NumberOfAdults { get; set; } = 1;

        [Range(0, 8, ErrorMessage = "Number of children must be between 0 and 8.")]
        public int NumberOfChildren { get; set; } = 0;

        [Range(0, 2, ErrorMessage = "Number of infants must be between 0 and 2.")]
        public int NumberOfInfants { get; set; } = 0;
    }
}

// File: FlightSearchResultDto.cs
namespace Application.DTOs.Flight
{
    public class FlightSearchResultDto
    {
        public int FlightInstanceId { get; set; }
        public string FlightNumber { get; set; }
        public string AirlineName { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string OriginAirportIata { get; set; }
        public string DestinationAirportIata { get; set; }
        public decimal BasePrice { get; set; }
        public int AvailableSeats { get; set; }
    }
}
 
namespace Application.DTOs.Flight
{
    public class CabinClassDto
    {
        public int CabinClassId { get; set; }
        public string Name { get; set; }
        public int AvailableSeats { get; set; }
        public decimal BaseFare { get; set; }
    }
}