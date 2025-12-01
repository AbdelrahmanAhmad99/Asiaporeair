using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Seat
    {
        [Key]
        [Column("seat_id")]
        [StringLength(20)]
        public string SeatId { get; set; }

        [Required]
        [Column("aircraft_fk")]
        [StringLength(10)]
        public string AircraftId { get; set; }

        [Required]
        [Column("seat_number")]
        [StringLength(10)]
        public string SeatNumber { get; set; }

        [Required]
        [Column("cabin_class_fk")]
        public int CabinClassId { get; set; }

        [Column("is_window")]
        public bool? IsWindow { get; set; }

        [Column("is_exit_row")]
        public bool? IsExitRow { get; set; }

        [Required]
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("AircraftId")]
        public Aircraft Aircraft { get; set; }

        [ForeignKey("CabinClassId")]
        public CabinClass CabinClass { get; set; } 
        public ICollection<BookingPassenger> BookingPassengers { get; set; } = new List<BookingPassenger>(); 
        public ICollection<BoardingPass> BoardingPasss { get; set; } = new List<BoardingPass>();
    }
}