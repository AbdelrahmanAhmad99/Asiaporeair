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
    public class FlightLegDef
    {
        [Key]
        [Column("leg_def_id")]
        public int LegDefId { get; set; }

        [Required]
        [Column("schedule_fk")]
        public int ScheduleId { get; set; }

        [Required]
        [Column("segment_number")]
        public int SegmentNumber { get; set; }

        [Required]
        [Column("departure_airport_fk")]
        [StringLength(3)]
        public string DepartureAirportId { get; set; }

        [Required]
        [Column("arrival_airport_fk")]
        [StringLength(3)]
        public string ArrivalAirportId { get; set; }

        [Required]
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("ScheduleId")]
        public FlightSchedule Schedule { get; set; }

        [ForeignKey("DepartureAirportId")]
        public Airport DepartureAirport { get; set; }

        [ForeignKey("ArrivalAirportId")]
        public Airport ArrivalAirport { get; set; }

        public ICollection<AncillarySale> AncillarySales { get; set; }
    }
}