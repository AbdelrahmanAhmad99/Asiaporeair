using System.Collections.Generic;

namespace Application.DTOs.Aircraft
{
    // Represents the full details of a single aircraft, including its configurations
    public class AircraftDetailDto : AircraftDto
    {
        public List<AircraftConfigDto> Configurations { get; set; } = new List<AircraftConfigDto>();
    }
}