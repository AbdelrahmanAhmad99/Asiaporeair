using System;
using System.Collections.Generic;

namespace Application.DTOs.Reporting
{
    // DTO for the flight occupancy / load factor report.
    public class LoadFactorReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int TotalFlightsOperated { get; set; }
        public int TotalCapacityOffered { get; set; } // Sum of MaxSeats for all operated flights
        public int TotalPassengersConfirmed { get; set; } // Sum of confirmed passengers

        public double AverageLoadFactorPercent { get; set; } // (TotalPassengers / TotalCapacity)

        // Breakdown by route
        public List<RouteLoadFactorDto> TopRoutesByLoadFactor { get; set; } = new List<RouteLoadFactorDto>();
        public List<RouteLoadFactorDto> BottomRoutesByLoadFactor { get; set; } = new List<RouteLoadFactorDto>();
    }

    // Helper DTO for LoadFactorReportDto
    public class RouteLoadFactorDto
    {
        public string RouteName { get; set; } = string.Empty; // e.g., "SIN-LHR"
        public int FlightsOnRoute { get; set; }
        public double LoadFactorPercent { get; set; }
        public int TotalPassengers { get; set; }
        public int TotalCapacity { get; set; }
    }
}