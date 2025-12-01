using System;
using System.Collections.Generic;

namespace Application.DTOs.Payment
{
    public class PaymentResultDto
    {
        public bool IsSuccess { get; set; }
        public string PaymentIntentId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public int BookingId { get; set; }
        public List<int> TicketIds { get; set; } = new List<int>(); // List of generated ticket IDs
        public string? ErrorMessage { get; set; }
    }
}