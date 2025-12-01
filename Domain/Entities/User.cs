using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("user")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Column("frequent_flyer_fk")]
        public int? FrequentFlyerId { get; set; }

        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("kris_flyer_tier")]
        [StringLength(50)]
        public string? KrisFlyerTier { get; set; }
         
        [Column("AppUserId")]
        [Required]
        public string AppUserId { get; set; }

        [ForeignKey("AppUserId")]
        public virtual AppUser AppUser { get; set; } = null!;

        // --- Navigation properties ---

        [ForeignKey("FrequentFlyerId")]
        public virtual FrequentFlyer? FrequentFlyer { get; set; }
         
        public virtual ICollection<Passenger> Passengers { get; set; } = new List<Passenger>();
         
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}