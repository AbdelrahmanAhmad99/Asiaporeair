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
    public class Booking
    {
        [Key]
        [Column("booking_id")]
        public int BookingId { get; set; }

        [Required]
        [Column("User_fk")]
        public int UserId { get; set; }

        [Required]
        [Column("flight_instance_fk")]
        public int FlightInstanceId { get; set; }

        [Required]
        [Column("booking_ref")]
        [StringLength(10)]
        public string BookingRef { get; set; }

        [Required]
        [Column("booking_time")]
        public DateTime BookingTime { get; set; }

        [Column("price_total")]
        public decimal? PriceTotal { get; set; }

        [Column("payment_status")]
        [StringLength(20)]
        public string PaymentStatus { get; set; }

        [Required]
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        [Required]
        [Column("fare_basis_code_fk")]
        [StringLength(10)]
        public string FareBasisCodeId { get; set; }

        [Column("PointsAwarded")]
        public bool PointsAwarded { get; set; } = false;

        // Navigation properties
        [ForeignKey("UserId")]
        public User User { get; set; }  


        [ForeignKey("FlightInstanceId")]
        public FlightInstance FlightInstance { get; set; }

        [ForeignKey("FareBasisCodeId")]
        public FareBasisCode FareBasisCode { get; set; }

        public ICollection<BookingPassenger> BookingPassengers { get; set; }
        public ICollection<Payment> Payments { get; set; }
        public ICollection<AncillarySale> AncillarySales { get; set; }
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}