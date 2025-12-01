using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Payment
{
    // This DTO is used by the Admin to request a refund.
    // It uses the *internal* PaymentId (int), not the gateway's string ID.
    public class RefundRequestDto
    {
        [Required]
        public int PaymentId { get; set; } // The internal database payment_id

        [Required]
        [StringLength(250, ErrorMessage = "Reason must be between 10 and 250 characters.", MinimumLength = 10)]
        public string Reason { get; set; } = string.Empty; // e.g., "Customer request", "Duplicate charge"
    }
}
