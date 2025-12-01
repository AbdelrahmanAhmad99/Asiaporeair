using System.ComponentModel.DataAnnotations;
namespace Application.DTOs.AncillaryProduct
{
    // DTO for displaying ancillary product details.
    public class AncillaryProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // e.g., Baggage, Meal, Seat, Insurance
        public decimal BaseCost { get; set; }
        public string UnitOfMeasure { get; set; } = string.Empty; // e.g., kg, piece, selection
    }
} 
namespace Application.DTOs.AncillaryProduct
{
    public class AncillaryPurchaseDto
    {
        public int SaleId { get; set; }
        public int BookingId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal PricePaid { get; set; }
        public int Quantity { get; set; }
    }
}