using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class FrequentFlyer
    {
        [Key]
        [Column("flyer_id")]
        public int FlyerId { get; set; }

        [Required]
        [Column("card_number")]
        [StringLength(50)]
        public string CardNumber { get; set; }

        [Column("level")]
        [StringLength(50)]
        public string Level { get; set; }

        [Column("award_points")]
        public int? AwardPoints { get; set; }

        [Required]
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public ICollection<User> User { get; set; }

    }
}
