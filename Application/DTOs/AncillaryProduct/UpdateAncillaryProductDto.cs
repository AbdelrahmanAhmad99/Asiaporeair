using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.AncillaryProduct
{
    // DTO for updating an ancillary product definition (Admin).
    public class UpdateAncillaryProductDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [Range(0, 10000)]
        public decimal BaseCost { get; set; }

        [Required]
        [StringLength(10)]
        public string UnitOfMeasure { get; set; } = string.Empty;
    }
}