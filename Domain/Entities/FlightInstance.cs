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
    public class FlightInstance
    {
        [Key]
        [Column("instance_id")]
        public int InstanceId { get; set; }

        [Required]
        [Column("schedule_fk")]
        public int ScheduleId { get; set; }
         
        [Column("aircraft_fk")]
        [StringLength(10)]
        public string? AircraftId { get; set; }

        [Required]
        [Column("scheduled_dep_ts")]
        public DateTime ScheduledDeparture { get; set; }

        [Column("actual_dep_ts")]
        public DateTime? ActualDeparture { get; set; }

        [Required]
        [Column("scheduled_arr_ts")]
        public DateTime ScheduledArrival { get; set; }

        [Column("actual_arr_ts")]
        public DateTime? ActualArrival { get; set; }

        [Column("status")]
        [StringLength(20)]
        public string Status { get; set; }

        [Required]
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("ScheduleId")]
        public FlightSchedule Schedule { get; set; }

        [ForeignKey("AircraftId")]
        public Aircraft Aircraft { get; set; } 
        public ICollection<Booking> Bookings { get; set; }
        public ICollection<FlightCrew> FlightCrews { get; set; }
    }
}