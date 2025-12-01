using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Payment
{
    // DTO for initiating a payment intent (Client -> Server)
    public class CreatePaymentIntentDto
    {
        [Required]
        public int BookingId { get; set; }

        // Optional: If you want to allow partial payments, otherwise the service takes the total from Booking
        public decimal? AmountOverride { get; set; }

        public string Currency { get; set; } = "sgd"; // Singapore Dollars by default
    }

    // DTO returned to the frontend to complete payment (Server -> Client)
    public class PaymentIntentResponseDto
    {
        public string PaymentIntentId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string PublishableKey { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
    }
     
}