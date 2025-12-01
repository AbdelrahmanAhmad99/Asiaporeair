using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.FrequentFlyer
{
    // DTO for manually adjusting points (admin action).
    public class UpdatePointsDto
    {
        [Required]
        public int PointsDelta { get; set; } // Positive to add, negative to subtract

        [Required]
        [StringLength(200)]
        public string Reason { get; set; } = string.Empty; // Reason for manual adjustment
    }
}