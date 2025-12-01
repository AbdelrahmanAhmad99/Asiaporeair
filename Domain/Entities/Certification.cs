using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Certification
    {
        [Key]
        [Column("cert_id")]
        public int CertId { get; set; }

        [Required]
        [Column("crew_member_fk")]
        public int CrewMemberId { get; set; }

        [Required]
        [Column("type")]
        [StringLength(50)]
        public string Type { get; set; }

        [Column("issue_date")]
        public DateTime? IssueDate { get; set; }

        [Column("expiry_date")]
        public DateTime? ExpiryDate { get; set; }

        [Required]
        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        // Navigation property
        [ForeignKey("CrewMemberId")]
        public CrewMember CrewMember { get; set; }
    }
}