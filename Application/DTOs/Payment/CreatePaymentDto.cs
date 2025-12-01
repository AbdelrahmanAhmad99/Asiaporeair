using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Payment
{
    // DTO representing the *intent* to pay for a specific booking using a chosen method.
    public class CreatePaymentDto
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        [StringLength(20)]
        public string Method { get; set; } = string.Empty; // e.g., "CreditCard", "PayPal"

        // Optional: Include specific details for the method if needed upfront
        // e.g., public string? CardToken { get; set; }
        // e.g., public string? RedirectUrl { get; set; } // URL to return to after external payment
    }
}
 