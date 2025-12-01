using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Crew
{
    // DTO specifically for creating a new certification record.
    public class CreateCertificationDto
    {
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty;

        public DateTime? IssueDate { get; set; }

        [Required(ErrorMessage = "Expiry date is required.")]
        public DateTime ExpiryDate { get; set; }
    }
}
