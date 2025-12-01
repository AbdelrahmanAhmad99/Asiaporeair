using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class RouteOperator
    { 
        [Key]
        [Column("route_fk", Order = 0)]
        public int RouteId { get; set; }

        [Key]
        [Column("airline_fk", Order = 1, TypeName = "nvarchar(2)")]
        [StringLength(2)]
        public string AirlineId { get; set; }

        [Column("codeshare_status")]
        public bool? CodeshareStatus { get; set; }

        [Required]
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("RouteId")]
        public Route Route { get; set; }

        [ForeignKey("AirlineId")]
        public Airline Airline { get; set; }
    }
}