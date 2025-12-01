namespace Application.DTOs.Payment
{
    public class RefundResultDto
    {
        public bool IsSuccess { get; set; }
        public string RefundId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}