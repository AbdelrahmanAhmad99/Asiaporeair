using System;

namespace Application.DTOs.Aircraft
{
    // DTO for advanced filtering of the aircraft fleet
    public class AircraftFilterDto
    {
        public string? AirlineIataCode { get; set; }
        public int? AircraftTypeId { get; set; }
        public string? Status { get; set; }
        public int? MinFlightHours { get; set; }
        public int? MaxFlightHours { get; set; }
        public DateTime? AcquiredAfterDate { get; set; }
        public bool IncludeDeleted { get; set; } = false;
    }
}