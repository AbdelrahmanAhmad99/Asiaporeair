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
    public class FareBasisCode
    {
        [Key]
        [Column("code")]
        [StringLength(10)]
        public string Code { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("rules")]
        public string Rules { get; set; }

        [Required]
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<PriceOfferLog> PriceOfferLogs { get; set; }
    }
}