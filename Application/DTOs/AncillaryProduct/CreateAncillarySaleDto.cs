using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.AncillaryProduct
{
    // DTO used when adding an ancillary product to an *existing* booking.
    public class CreateAncillarySaleDto
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Range(1, 100)]
        public int Quantity { get; set; } = 1;

        //public int? PassengerId { get; set; } // Specify passenger if needed
        //public int? SegmentId { get; set; }   // Specify flight leg if needed
    }
}