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
    public class Aircraft
    {
        [Key]
        [Column("tail_number")]
        [StringLength(10)]
        public string TailNumber { get; set; }

        [Required]
        [Column("airline_fk")]
        [StringLength(2)]
        public string AirlineId { get; set; }

        [Required]
        [Column("aircraft_type_fk")]
        public int AircraftTypeId { get; set; }

        [Column("total_flight_hours")]
        public int? TotalFlightHours { get; set; }

        [Column("acquisition_date")]
        public DateTime? AcquisitionDate { get; set; }

        [Column("status")]
        [StringLength(20)]
        public string Status { get; set; }

        [Required]
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;
         
        [ForeignKey("AirlineId")]
        public Airline Airline { get; set; }

        [ForeignKey("AircraftTypeId")]
        public AircraftType AircraftType { get; set; } 
        public ICollection<AircraftConfig> Configurations { get; set; }
        public ICollection<Seat> Seats { get; set; }
        public ICollection<FlightInstance> FlightInstances { get; set; }
    }
}
