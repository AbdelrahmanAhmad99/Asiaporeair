using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("Supervisors")]
    public class Supervisor
    {
        [Key]
        [Column("AppUserId")]
        public string AppUserId { get; set; }

        [Column("EmployeeId")]
        public int EmployeeId { get; set; } 
        public string? AddedById { get; set; }

        [StringLength(100)]
        public string? ManagedArea { get; set; }

        // --- Navigation properties ---
        [ForeignKey("AppUserId")]
        public virtual AppUser AppUser { get; set; } = null!;

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;

        [ForeignKey("AddedById")]
        public virtual AppUser? AddedBy { get; set; }
    }
}