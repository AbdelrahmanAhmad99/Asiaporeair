using System;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Crew
{
    // DTO for displaying or creating/updating a certification.
    public class CertificationDto
    {
        public int CertId { get; set; }
        public int CrewMemberId { get; set; }
        [Required]
        [StringLength(50)]
        public string Type { get; set; } = string.Empty; // e.g., "Medical Class 1", "Type Rating B777"

        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool IsExpired { get; set; } // Calculated property
    }
}