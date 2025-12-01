using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.FlightOperations
{
    // DTO for airport staff to update gate and baggage information.
    public class UpdateGateInfoDto
    {
        [StringLength(10)]
        public string? DepartureGate { get; set; }

        [StringLength(10)]
        public string? ArrivalGate { get; set; }

        [StringLength(10)]
        public string? BaggageCarousel { get; set; }
    }
}