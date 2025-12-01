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
    public class Airline
    {
        [Key]
        [Column("iata_code")]
        [StringLength(2)]
        public string IataCode { get; set; }

        [Required]
        [Column("name")]
        [StringLength(100)]
        public string Name { get; set; }

        [Column("callsign")]
        [StringLength(50)]
        public string Callsign { get; set; }

        [Column("operating_region")]
        [StringLength(50)]
        public string OperatingRegion { get; set; }

        [Required]
        [Column("base_airport_fk")]
        [StringLength(3)]
        public string BaseAirportId { get; set; }

        [Required]
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("BaseAirportId")]
        public Airport BaseAirport { get; set; }

        public ICollection<Aircraft> Aircrafts { get; set; }
        public ICollection<RouteOperator> RouteOperators { get; set; }
        public ICollection<FlightSchedule> FlightSchedules { get; set; }
    }
}