using System.Collections.Generic;
using Application.DTOs.Flight;

namespace Application.DTOs.Aircraft
{
    // Represents a specific seating configuration for an aircraft
    public class AircraftConfigDto
    {
        public int ConfigId { get; set; }
        public string ConfigurationName { get; set; } = string.Empty;
        public int? TotalSeatsCount { get; set; }
        public List<CabinClassDto> CabinClasses { get; set; } = new List<CabinClassDto>();
    }
}