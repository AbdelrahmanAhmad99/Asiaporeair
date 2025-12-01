using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Payment
{
    public class RefundDto
    {
        [Required]
        public string PaymentIntentId { get; set; } = string.Empty;

        public decimal? Amount { get; set; } // Optional: for partial refunds

        [Required]
        public string Reason { get; set; } = "requested_by_customer"; // e.g., "duplicate", "fraudulent"
    }
}