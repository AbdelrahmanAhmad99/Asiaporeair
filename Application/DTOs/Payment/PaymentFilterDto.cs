using System;

namespace Application.DTOs.Payment
{
    // DTO for searching/filtering payments (Admin).
    public class PaymentFilterDto
    {
        public int? BookingId { get; set; }
        public string? BookingReference { get; set; }
        public string? Method { get; set; }
        public string? Status { get; set; }
        public string? TransactionId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public bool IncludeDeleted { get; set; } = false;
    }
}