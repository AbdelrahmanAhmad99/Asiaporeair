namespace Application.DTOs.AncillaryProduct
{
    // DTO for displaying details of a purchased ancillary item within a booking.
    public class AncillarySaleDto // Renamed from AncillaryPurchaseDto
    {
        public int SaleId { get; set; }
        public int BookingId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal PricePaid { get; set; } // Price *at the time of purchase*
        public int Quantity { get; set; }
        public int? SegmentId { get; set; }
        //public int? PassengerId { get; set; } // Add if needed
    }
}