using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class ContextualPricingAttributes
    {
        [Key]
        [Column("attribute_id")]
        public int AttributeId { get; set; }

        [Column("time_until_departure")]
        public int? TimeUntilDeparture { get; set; }

        [Column("length_of_stay")]
        public int? LengthOfStay { get; set; }

        [Column("competitor_fares")]
        public string CompetitorFares { get; set; }

        [Column("willingness_to_pay")]
        public decimal? WillingnessToPay { get; set; }

        [Required]
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public ICollection<PriceOfferLog> PriceOfferLogs { get; set; }
    }
}