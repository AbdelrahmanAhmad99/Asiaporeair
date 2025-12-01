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
    public class FlightSchedule
    {
        [Key]
        [Column("schedule_id")]
        public int ScheduleId { get; set; }

        [Required]
        [Column("flight_no")]
        [StringLength(10)]
        public string FlightNo { get; set; }

        [Required]
        [Column("route_fk")]
        public int RouteId { get; set; }

        [Required]
        [Column("airline_fk")]
        [StringLength(2)]
        public string AirlineId { get; set; }

        [Required]
        [Column("aircraft_type_fk")]
        public int AircraftTypeId { get; set; }

        [Required]
        [Column("departure_time_scheduled")]
        public DateTime DepartureTimeScheduled { get; set; }

        [Required]
        [Column("arrival_time_scheduled")]
        public DateTime ArrivalTimeScheduled { get; set; }

        [Column("days_of_week")]
        public byte? DaysOfWeek { get; set; }

        [Required]
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("RouteId")]
        public Route Route { get; set; }

        [ForeignKey("AirlineId")]
        public Airline Airline { get; set; }

        [ForeignKey("AircraftTypeId")]
        public AircraftType AircraftType { get; set; }

        public ICollection<FlightLegDef> FlightLegs { get; set; }
        public ICollection<FlightInstance> FlightInstances { get; set; }
    }
}