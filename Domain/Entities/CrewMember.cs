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
    public class CrewMember
    {
        [Key]
        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [Required]
        [Column("crew_base_airport_fk")]
        [StringLength(3)]
        public string CrewBaseAirportId { get; set; }

        [Column("position")]
        [StringLength(50)]
        public string Position { get; set; }

         
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("EmployeeId")]
        public Employee Employee { get; set; }

        [ForeignKey("CrewBaseAirportId")]
        public Airport CrewBaseAirport { get; set; }

        public Pilot? Pilot { get; set; }
        public Attendant? Attendant { get; set; }
        public ICollection<Certification> Certifications { get; set; }
        public ICollection<FlightCrew> FlightCrews { get; set; }
    }
}