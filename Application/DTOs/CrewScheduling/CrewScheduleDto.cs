using System;
using System.Collections.Generic;

namespace Application.DTOs.CrewScheduling
{
    // DTO representing a crew member's assignments over a period.
    public class CrewScheduleDto
    {
        public int CrewMemberEmployeeId { get; set; }
        public string CrewMemberName { get; set; } = string.Empty;
        public DateTime ScheduleStartDate { get; set; }
        public DateTime ScheduleEndDate { get; set; }
        public List<ScheduledFlightDto> AssignedFlights { get; set; } = new List<ScheduledFlightDto>();
    }

    // Represents a flight within a crew member's schedule.
    public class ScheduledFlightDto
    {
        public int FlightInstanceId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string OriginAirport { get; set; } = string.Empty;
        public string DestinationAirport { get; set; } = string.Empty;
        public DateTime ScheduledDeparture { get; set; }
        public DateTime ScheduledArrival { get; set; }
        public string AssignedRole { get; set; } = string.Empty;
        public string AircraftType { get; set; } = string.Empty;
    }
}