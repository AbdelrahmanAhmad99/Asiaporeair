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
    public class AircraftType
    {
        [Key]
        [Column("type_id")]
        public int TypeId { get; set; }

        [Required]
        [Column("model")]
        [StringLength(50)]
        public string Model { get; set; }

        [Required]
        [Column("manufacturer")]
        [StringLength(50)]
        public string Manufacturer { get; set; }

        [Column("range_km")]
        public int? RangeKm { get; set; }

        [Column("max_seats")]
        public int? MaxSeats { get; set; }

        [Column("cargo_capacity")]
        public decimal? CargoCapacity { get; set; }

        [Column("cruising_velocity")]
        public int? CruisingVelocity { get; set; }

        [Required]
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        public ICollection<Aircraft> Aircrafts { get; set; }
        public ICollection<FlightSchedule> FlightSchedules { get; set; }
        public ICollection<Pilot> Pilots { get; set; }
    }
}