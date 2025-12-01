using System;

namespace Application.DTOs.Payment
{
    // DTO for displaying payment details.
    public class PaymentDto
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }
        public string BookingReference { get; set; } = string.Empty; // Added for context
        public decimal Amount { get; set; }
        public string Method { get; set; } = string.Empty; // e.g., "CreditCard", "KrisFlyerPoints", "Voucher"
        public string Status { get; set; } = string.Empty; // e.g., Pending, Success, Failed, Refunded
        public string? TransactionId { get; set; } // From payment gateway
        public DateTime TransactionDateTime { get; set; }
    }
}