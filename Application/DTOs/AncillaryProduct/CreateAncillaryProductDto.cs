using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.AncillaryProduct
{
    // DTO for creating a new ancillary product definition (Admin).
    public class CreateAncillaryProductDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [Range(0, 10000, ErrorMessage = "Base cost must be non-negative and realistic.")]
        public decimal BaseCost { get; set; }

        [Required]
        [StringLength(10)]
        public string UnitOfMeasure { get; set; } = string.Empty;
    }
}