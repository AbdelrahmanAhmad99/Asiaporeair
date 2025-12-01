using System.Collections.Generic;

namespace Application.DTOs.Seat
{
    // DTO representing the seat layout for a specific flight instance.
    public class SeatMapDto
    {
        public int FlightInstanceId { get; set; }
        public string AircraftModel { get; set; } = string.Empty;
        public string AircraftTailNumber { get; set; } = string.Empty;
        // Group seats by cabin for easier rendering
        public List<CabinSeatLayoutDto> CabinLayouts { get; set; } = new List<CabinSeatLayoutDto>();
    }

    // Represents the layout within a single cabin.
    public class CabinSeatLayoutDto
    {
        public int CabinClassId { get; set; }
        public string CabinClassName { get; set; } = string.Empty;
        // Could add Row/Column info here if needed for precise layout
        public List<SeatDto> Seats { get; set; } = new List<SeatDto>();
    }
}