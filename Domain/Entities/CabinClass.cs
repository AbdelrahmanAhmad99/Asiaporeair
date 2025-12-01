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
    public class CabinClass
    {
        [Key]
        [Column("cabin_class_id")]
        public int CabinClassId { get; set; }

        [Required]
        [Column("config_fk")]
        public int ConfigId { get; set; }

        [Required]
        [Column("name")]
        [StringLength(20)]
        public string Name { get; set; }

        [Required]
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation properties
        [ForeignKey("ConfigId")]
        public AircraftConfig AircraftConfig { get; set; }

        public ICollection<Seat> Seats { get; set; }
    }
}