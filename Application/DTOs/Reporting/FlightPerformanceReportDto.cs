using System;

namespace Application.DTOs.Reporting
{
    // DTO for the operational flight performance report.
    public class FlightPerformanceReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? AirportIataCode { get; set; } // Report context

        public int TotalFlightsScheduled { get; set; } // Total flights in system for this period
        public int TotalFlightsOperated { get; set; } // Total - Cancelled
        public int FlightsCancelled { get; set; }
         
        public int TotalDepartures { get; set; } // Total departures *from the specified airport*
        public int TotalArrivals { get; set; }   // Total arrivals *to the specified airport*
                                                 

        public int DeparturesDelayed { get; set; } // e.g., > 15 mins
        public int ArrivalsDelayed { get; set; }   // e.g., > 15 mins

        public double OnTimeDeparturePercentage { get; set; }
        public double OnTimeArrivalPercentage { get; set; }

        public double AverageDepartureDelayMinutes { get; set; }
        public double AverageArrivalDelayMinutes { get; set; }
    }
}