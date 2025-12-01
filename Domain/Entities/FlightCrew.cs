using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class FlightCrew
    {
      
        [Key]
        [Column("flight_instance_fk", Order = 0)]
        public int FlightInstanceId { get; set; }

        [Key]
        [Column("crew_member_fk", Order = 1)]
        public int CrewMemberId { get; set; }

        [Column("role")]
        [StringLength(50)]
        public string Role { get; set; }

        [Required]
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("FlightInstanceId")]
        public FlightInstance FlightInstance { get; set; }

        [ForeignKey("CrewMemberId")]
        public CrewMember CrewMember { get; set; }
    }
}