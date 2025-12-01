using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Payment
{
    // DTO representing the data received from a payment gateway callback/webhook.
    public class PaymentCallbackDto
    {
        [Required(ErrorMessage = "Internal Payment ID is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Internal Payment ID must be greater than 0.")]
        public int InternalPaymentId { get; set; }

        [Required(ErrorMessage = "Gateway Transaction ID is required.")]
        public string GatewayTransactionId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Gateway Status is required.")]
        public string GatewayStatus { get; set; } = string.Empty; // e.g., "succeeded", "failed"

        public string? FailureReason { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }
    }
}