using System;
using System.Collections.Generic;

namespace Application.DTOs.Reporting
{
    // DTO for the main Sales Summary report.
    public class SalesSummaryReportDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? AirlineIataCode { get; set; } // Report context

        public int TotalBookingsConfirmed { get; set; }
        public int TotalBookingsCancelled { get; set; }
        public int TotalPassengers { get; set; }

        public decimal TotalBookingRevenue { get; set; }
        public decimal TotalAncillaryRevenue { get; set; }
        public decimal TotalRevenue => TotalBookingRevenue + TotalAncillaryRevenue;

        public decimal AverageRevenuePerBooking { get; set; }

        // Breakdown of sales
        public Dictionary<string, int> BookingsByFareCode { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, decimal> RevenueByFareCode { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, int> TopRoutesByBookings { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, decimal> TopRoutesByRevenue { get; set; } = new Dictionary<string, decimal>();
        public Dictionary<string, int> TopAncillariesByQuantity { get; set; } = new Dictionary<string, int>();
    }
}