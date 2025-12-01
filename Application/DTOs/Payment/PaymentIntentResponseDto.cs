namespace Application.DTOs.Payment
{
    // DTO returned after successfully initiating a payment process.
    public class PaymentInitiationResponseDto
    {
        public int PaymentId { get; set; } // The internal ID for this payment attempt
        public string Status { get; set; } = "Pending"; // Initial status
        public string? PaymentGatewayUrl { get; set; } // URL to redirect user to (if applicable)
        public string? ClientSecret { get; set; } // For client-side SDKs like Stripe Elements
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "SGD"; // Default currency
    }
}