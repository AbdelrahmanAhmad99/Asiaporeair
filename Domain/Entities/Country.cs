using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Country
    {
        [Key]
        [Column("iso_code")]
        [StringLength(3)]
        public string IsoCode { get; set; }

        [Required]
        [Column("name")]
        [StringLength(100)]
        public string Name { get; set; }

        [Column("continent_fk")]
        [StringLength(50)]
        public string Continent { get; set; }

        [Required]
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation property for related airports
        public ICollection<Airport> Airports { get; set; }
    }
}
