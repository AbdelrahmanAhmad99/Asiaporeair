using System;

namespace Application.DTOs.Aircraft
{
    // Represents a single aircraft in a list or summary view
    public class AircraftDto
    {
        public string TailNumber { get; set; } = string.Empty;
        public string AirlineIataCode { get; set; } = string.Empty;
        public string AirlineName { get; set; } = string.Empty;
        public string AircraftTypeModel { get; set; } = string.Empty;
        public int AircraftTypeId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int? TotalFlightHours { get; set; }
        public DateTime? AcquisitionDate { get; set; }
    }
}