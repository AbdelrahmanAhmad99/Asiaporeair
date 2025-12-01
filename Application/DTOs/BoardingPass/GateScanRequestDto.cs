using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.BoardingPass
{
    // DTO representing data scanned at the gate (e.g., from barcode).
    public class GateScanRequestDto
    {
        [Required]
        public int PassId { get; set; } // Assuming PassId is encoded or looked up

        [Required]
        public int FlightInstanceId { get; set; } // Verify against current flight

        // Optional: Can add Gate number if validation needed
        // public string GateNumber { get; set; }
    }
}