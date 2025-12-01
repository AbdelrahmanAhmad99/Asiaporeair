using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.BoardingPass
{
    // DTO to request generation of a boarding pass (e.g., during check-in).
    public class GenerateBoardingPassRequestDto
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        public int PassengerId { get; set; }

        // Optionally allow specifying a seat if not pre-assigned, although seat assignment should ideally precede this.
        // public string? PreferredSeatId { get; set; }
    }
}