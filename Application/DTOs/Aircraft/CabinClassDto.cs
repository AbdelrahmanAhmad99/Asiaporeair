namespace Application.DTOs.Aircraft
{
    // Represents a cabin class within a specific configuration
    public class CabinClassDto
    {
        public int CabinClassId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int SeatCount { get; set; } // Calculated field
    }
}