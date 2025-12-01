using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class BoardingPass
    {
        [Key]
        [Column("pass_id")]
        public int PassId { get; set; }

        [Required]
        [Column("booking_passenger_booking_id")]
        public int BookingPassengerBookingId { get; set; }

        [Required]
        [Column("booking_passenger_passenger_id")]
        public int BookingPassengerPassengerId { get; set; }

        [Column("seat_fk")]
        [StringLength(20)]
        public string SeatId { get; set; }

        [Column("boarding_time")]
        public DateTime? BoardingTime { get; set; }

        [Column("precheck_status")]
        public bool? PrecheckStatus { get; set; }

        [Required]
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("BookingPassengerBookingId, BookingPassengerPassengerId")]
        public BookingPassenger BookingPassenger { get; set; }

        [ForeignKey("SeatId")]
        public Seat Seat { get; set; }
    }
}