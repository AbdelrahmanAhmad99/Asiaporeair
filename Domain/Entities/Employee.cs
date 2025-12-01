using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("employee")]
    public class Employee
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EmployeeId { get; set; }

        [Column("date_of_hire", TypeName = "DATE")]
        public DateTime? DateOfHire { get; set; }

        [Column("salary", TypeName = "DECIMAL(10,2)")]
        public decimal? Salary { get; set; }

        [Column("shift_preference_fk")]
        public int? ShiftPreferenceFk { get; set; }

        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;
         
        [Column("AppUserId")]
        [Required]
        public string AppUserId { get; set; } = string.Empty;
        
        [ForeignKey("AppUserId")]
        public virtual AppUser AppUser { get; set; } = null!;
        
        // --- Navigation properties to related employee roles ---

        // 1-to-1 relationship with CrewMember
        public virtual CrewMember? CrewMember { get; set; }

        // 1-to-1 relationship with Admin
        public virtual Admin? Admin { get; set; }

        // 1-to-1 relationship with SuperAdmin
        public virtual SuperAdmin? SuperAdmin { get; set; }
        
        // 1-to-1 relationship with Supervisor
        public virtual Supervisor? Supervisor { get; set; }
    }
}