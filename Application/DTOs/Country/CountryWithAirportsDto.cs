using Application.DTOs.Airport;
using System.Collections.Generic;

namespace Application.DTOs.Country
{
    // A detailed DTO for a Country that includes its list of airports.
    public class CountryWithAirportsDto
    {
        public string IsoCode { get; set; }
        public string Name { get; set; }
        public string Continent { get; set; }
        public IEnumerable<AirportBriefDto> Airports { get; set; }
    }
}