using System.ComponentModel.DataAnnotations;
namespace Application.DTOs.Seat
{
    // DTO representing detailed information about a single seat on an aircraft.
    public class SeatDto
    {
        public string SeatId { get; set; } = string.Empty; // e.g., "12A-B77W-Y" (SeatNum-AircraftType-CabinCode)
        public string SeatNumber { get; set; } = string.Empty; // e.g., "12A"
        public int CabinClassId { get; set; }
        public string CabinClassName { get; set; } = string.Empty; // e.g., "Economy"
        public string AircraftTailNumber { get; set; } = string.Empty;
        public bool IsWindow { get; set; }
        public bool IsAisle { get; set; } // Added for clarity
        public bool IsExitRow { get; set; }
        // public string Features { get; set; } // e.g., "Extra Legroom", "Bassinet" - Could be added
        public bool IsAvailable { get; set; } = true; // Indicates if reservable for the requested flight
        public decimal? SeatPrice { get; set; } // Price if it's a paid seat selection
    }
}
 
namespace Application.DTOs.Seat
{
    public class ReserveSeatDto
    {
        [Required]
        public string SeatId { get; set; }

        [Required]
        public int PassengerId { get; set; }
    }
}