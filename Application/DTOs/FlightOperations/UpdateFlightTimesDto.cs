using System;

namespace Application.DTOs.FlightOperations
{
    // DTO for updating estimated or actual flight times (from ATC or ground crew).
    public class UpdateFlightTimesDto
    {
        //public DateTime? EstimatedDeparture { get; set; }
        public DateTime? ActualDeparture { get; set; }
        //public DateTime? EstimatedArrival { get; set; }
        public DateTime? ActualArrival { get; set; }
    }
}