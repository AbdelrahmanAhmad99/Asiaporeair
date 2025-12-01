using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class BookingPassenger
    { 

        [Key, Column("booking_id", Order = 0)]
        public int BookingId { get; set; }

        [Key, Column("passenger_id", Order = 1)]
        public int PassengerId { get; set; }


        [Column("seat_assignment_fk")]
        [StringLength(20)]
        public string? SeatAssignmentId { get; set; }

        [Required]
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("BookingId")]
        public Booking Booking { get; set; }

        [ForeignKey("PassengerId")]
        public Passenger Passenger { get; set; }

        [ForeignKey("SeatAssignmentId")]
        public Seat SeatAssignment { get; set; }

        public ICollection<BoardingPass> BoardingPasses { get; set; }
    }
}
