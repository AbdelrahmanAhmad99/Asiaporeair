using System;
using System.Collections.Generic;

namespace Application.DTOs.CrewScheduling
{
    // DTO showing the complete crew roster for a specific flight instance.
    public class FlightRosterDto
    {
        public int FlightInstanceId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string OriginAirport { get; set; } = string.Empty;
        public string DestinationAirport { get; set; } = string.Empty;
        public DateTime ScheduledDeparture { get; set; }
        public DateTime ScheduledArrival { get; set; }
        public string AircraftType { get; set; } = string.Empty;
        public List<FlightCrewAssignmentDto> AssignedCrew { get; set; } = new List<FlightCrewAssignmentDto>();
    }
}