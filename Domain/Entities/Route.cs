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
    public class Route
    {
        [Key]
        [Column("route_id")]
        public int RouteId { get; set; }

        [Required]
        [Column("origin_airport_fk")]
        [StringLength(3)]
        public string OriginAirportId { get; set; }

        [Required]
        [Column("destination_airport_fk")]
        [StringLength(3)]
        public string DestinationAirportId { get; set; }

        [Column("distance_km")]
        public int? DistanceKm { get; set; }

        [Required]
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("OriginAirportId")]
        public Airport OriginAirport { get; set; }

        [ForeignKey("DestinationAirportId")]
        public Airport DestinationAirport { get; set; }

        public ICollection<RouteOperator> RouteOperators { get; set; }
        public ICollection<FlightSchedule> FlightSchedules { get; set; }
    }
}