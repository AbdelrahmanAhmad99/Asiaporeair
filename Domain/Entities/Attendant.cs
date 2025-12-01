using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("attendant")]
    public class Attendant
    {
        [Key]
        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("AppUserId")]
        [Required]
        public string AppUserId { get; set; } = string.Empty;
         
        public string? AddedById { get; set; }

        // --- Navigation properties ---
        [ForeignKey("EmployeeId")]
        public virtual CrewMember CrewMember { get; set; } = null!;

        [ForeignKey("AppUserId")]
        public virtual AppUser AppUser { get; set; } = null!;

        [ForeignKey("AddedById")]
        public virtual AppUser? AddedBy { get; set; }
    }
}