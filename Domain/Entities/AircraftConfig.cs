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
    public class AircraftConfig
    {
        [Key]
        [Column("config_id")]
        public int ConfigId { get; set; }

        [Required]
        [Column("aircraft_fk")]
        [StringLength(10)]
        public string AircraftId { get; set; }

        [Required]
        [Column("configuration_name")]
        [StringLength(50)]
        public string ConfigurationName { get; set; }

        [Column("total_seats_count")]
        public int? TotalSeatsCount { get; set; }

        [Required]
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("AircraftId")]
        public Aircraft Aircraft { get; set; }

        public ICollection<CabinClass> CabinClasses { get; set; }
    }
}