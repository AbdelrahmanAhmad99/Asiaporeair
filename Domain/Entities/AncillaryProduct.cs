using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class AncillaryProduct
    {
        [Key]
        [Column("product_id")]
        public int ProductId { get; set; }

        [Required]
        [Column("name")]
        [StringLength(50)]
        public string Name { get; set; }

        [Column("category")]
        [StringLength(20)]
        public string Category { get; set; }

        [Column("base_cost")]
        public decimal? BaseCost { get; set; }

        [Column("unit_of_measure")]
        [StringLength(10)]
        public string UnitOfMeasure { get; set; }

        [Required]
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public ICollection<AncillarySale> AncillarySales { get; set; }
        public ICollection<PriceOfferLog> PriceOfferLogs { get; set; }
    }
}