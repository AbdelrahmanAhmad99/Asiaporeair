namespace Application.DTOs.Flight
{
    // Helper DTO for FlightDetailsDto, showing seat info for a cabin class.
    public class CabinClassAvailabilityDto
    {
        public int CabinClassId { get; set; }
        public string Name { get; set; } = string.Empty; // e.g., "Business Class"
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }
        public string Layout { get; set; } = "1-2-1"; // Example
    }
}