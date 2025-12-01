using System;

namespace Application.DTOs.FlightOperations
{
    // DTO for displaying detailed information about a single, specific flight instance.
    // This is the "real" flight happening on a specific date.
    public class FlightInstanceDto
    {
        public int InstanceId { get; set; }
        public int ScheduleId { get; set; }
        public string FlightNo { get; set; } = string.Empty;

        // Route & Airline Details (from Schedule)
        public string AirlineIataCode { get; set; } = string.Empty;
        public string AirlineName { get; set; } = string.Empty;
        public string OriginIataCode { get; set; } = string.Empty;
        public string OriginAirportName { get; set; } = string.Empty;
        public string OriginCity { get; set; } = string.Empty;
        public string DestinationIataCode { get; set; } = string.Empty;
        public string DestinationAirportName { get; set; } = string.Empty;
        public string DestinationCity { get; set; } = string.Empty;

        // Scheduled Times (from Instance)
        public DateTime ScheduledDeparture { get; set; }
        public DateTime ScheduledArrival { get; set; }

        // Actual/Estimated Times (Operational)
        //public DateTime? EstimatedDeparture { get; set; }
        public DateTime? ActualDeparture { get; set; }
        //public DateTime? EstimatedArrival { get; set; }
        public DateTime? ActualArrival { get; set; }

        // Operational Status
        public string Status { get; set; } = "Scheduled"; // e.g., Scheduled, OnTime, Delayed, Departed, Arrived, Cancelled

        // Assigned Aircraft (Operational)
        public string? AssignedAircraftTailNumber { get; set; }
        public string ScheduledAircraftModel { get; set; } = string.Empty; // From Schedule
        public string? AssignedAircraftModel { get; set; } // From actual aircraft, if different

        // Airport Operational Details
    //    public string? DepartureGate { get; set; }
    //    public string? ArrivalGate { get; set; }
    //    public string? BaggageCarousel { get; set; }
    }
}