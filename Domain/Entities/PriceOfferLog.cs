using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class PriceOfferLog
    {
        [Key]
        [Column("offer_id")]
        public int OfferId { get; set; }

        [Column("product_fk")]
        public int? ProductId { get; set; }

        [Required]
        [Column("offer_price_quote")]
        public decimal OfferPriceQuote { get; set; }

        [Required]
        [Column("timestamp")]
        public DateTime Timestamp { get; set; }

        [Required]
        [Column("context_attributes_fk")]
        public int ContextAttributesId { get; set; }

        [Column("fare_fk")]
        [StringLength(10)]
        public string? FareId { get; set; }

        [Column("ancillary_fk")]
        public int? AncillaryId { get; set; }

        [Required]
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("ContextAttributesId")]
        public ContextualPricingAttributes ContextAttributes { get; set; }

        [ForeignKey("FareId")]
        public FareBasisCode Fare { get; set; }

        [ForeignKey("AncillaryId")]
        public AncillaryProduct Ancillary { get; set; }
    }
}
