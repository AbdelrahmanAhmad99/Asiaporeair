using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class AncillarySale
    {
        [Key]
        [Column("sale_id")]
        public int SaleId { get; set; }

        [Required]
        [Column("booking_fk")]
        public int BookingId { get; set; }

        [Required]
        [Column("product_fk")]
        public int ProductId { get; set; }

        [Column("quantity")]
        public int? Quantity { get; set; }

        [Column("price_paid")]
        public decimal? PricePaid { get; set; }

        [Column("segment_fk")]
        public int? SegmentId { get; set; }

        [Required]
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("BookingId")]
        public Booking Booking { get; set; }

        [ForeignKey("ProductId")]
        public AncillaryProduct Product { get; set; }

        [ForeignKey("SegmentId")]
        public FlightLegDef Segment { get; set; }
    }
}