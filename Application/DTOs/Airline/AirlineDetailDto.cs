using Application.DTOs.Aircraft;  
using System.Collections.Generic;

namespace Application.DTOs.Airline
{
    // Detailed DTO including the airline's fleet
    public class AirlineDetailDto : AirlineDto
    {
        public List<AircraftDto> Fleet { get; set; } = new List<AircraftDto>(); // List of aircraft in the fleet
    }
}

  