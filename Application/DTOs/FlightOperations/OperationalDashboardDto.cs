using System;
using System.Collections.Generic;

namespace Application.DTOs.FlightOperations
{
    // DTO for the main airport operational dashboard (Changi Airport View).
    public class OperationalDashboardDto
    {
        public string AirportIataCode { get; set; } = string.Empty;
        public DateTime ForDate { get; set; }

        public int TotalDepartures { get; set; }
        public int TotalArrivals { get; set; }

        public int DeparturesOnTime { get; set; }
        public int DeparturesDelayed { get; set; }
        public int DeparturesCancelled { get; set; }

        public int ArrivalsOnTime { get; set; }
        public int ArrivalsDelayed { get; set; }
        public int ArrivalsCancelled { get; set; }

        public int AircraftOnGround { get; set; }

        public List<FlightInstanceBriefDto> UrgentFlights { get; set; } = new List<FlightInstanceBriefDto>();
    }
}