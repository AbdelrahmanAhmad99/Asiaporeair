using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Routing;

namespace Domain.Entities
{
    public class Airport
    {
        [Key]
        [Column("iata_code")]
        [StringLength(3)]
        public string IataCode { get; set; }

        [Required]
        [Column("icao_code")]
        [StringLength(4)]
        public string IcaoCode { get; set; }

        [Required]
        [Column("name")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [Column("city")]
        [StringLength(100)]
        public string City { get; set; }

        [Required]
        [Column("country_fk")]
        [StringLength(3)]
        public string CountryId { get; set; }

        [Required]
        [Column("latitude")]
        public decimal Latitude { get; set; }

        [Required]
        [Column("longitude")]
        public decimal Longitude { get; set; }

        [Column("altitude")]
        public int? Altitude { get; set; }

        [Required]
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("CountryId")]
        public Country Country { get; set; }

        public ICollection<Airline> Airlines { get; set; }
        public ICollection<Route> OriginRoutes { get; set; }
        public ICollection<Route> DestinationRoutes { get; set; }
        public ICollection<FlightLegDef> DepartureLegs { get; set; }
        public ICollection<FlightLegDef> ArrivalLegs { get; set; }
        public ICollection<CrewMember> CrewMembers { get; set; }
    }
}