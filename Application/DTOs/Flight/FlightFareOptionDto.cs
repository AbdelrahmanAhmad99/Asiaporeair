namespace Application.DTOs.Flight
{
    // Represents a specific fare package (e.g., Economy Lite, Business Flex)
    // for a given flight itinerary.
    public class FlightFareOptionDto
    {
        public string FareBasisCode { get; set; } = string.Empty;
        public string FareName { get; set; } = string.Empty; // e.g., "Economy Lite"
        public string CabinClass { get; set; } = string.Empty; // e.g., "Economy"

        // The *total price per passenger* for this fare
        public decimal PricePerAdult { get; set; }
        public decimal PricePerChild { get; set; }
        public decimal PricePerInfant { get; set; }

        // Key benefits/rules
        public string BaggageAllowance { get; set; } = string.Empty; // e.g., "25kg"
        public bool IsChangeable { get; set; } = false;
        public bool IsRefundable { get; set; } = false;

        public int AvailableSeats { get; set; }
    }
}