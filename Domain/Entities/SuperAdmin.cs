using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("SuperAdmins")]
    public class SuperAdmin
    {
        [Key]
        [Column("AppUserId")]
        public string AppUserId { get; set; }

        [Column("EmployeeId")]
        public int EmployeeId { get; set; }

        // --- Navigation properties ---

        [ForeignKey("AppUserId")]
        public virtual AppUser AppUser { get; set; } = null!;

        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; } = null!;
    }
}