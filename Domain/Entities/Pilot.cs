using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    [Table("pilot")]
    public class Pilot
    {
        [Key]
        [Column("employee_id")]
        public int EmployeeId { get; set; }

        [Column("license_number")]
        [Required]
        [StringLength(20)]
        public string LicenseNumber { get; set; } = string.Empty;

        [Column("total_flight_hours")]
        public int? TotalFlightHours { get; set; }

        [Column("type_rating_fk")]
        public int AircraftTypeId { get; set; }

        [Column("last_sim_check_date", TypeName = "DATE")]
        public DateTime? LastSimCheckDate { get; set; }

        [Column("IsDeleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("AppUserId")]
        [Required]
        public string AppUserId { get; set; } = string.Empty;
         
        public string? AddedById { get; set; }

        // --- Navigation properties ---
        [ForeignKey("EmployeeId")]
        public virtual CrewMember CrewMember { get; set; } = null!;

        [ForeignKey("AircraftTypeId")]
        public virtual AircraftType TypeRating { get; set; } = null!;

        [ForeignKey("AppUserId")]
        public virtual AppUser AppUser { get; set; } = null!;

        [ForeignKey("AddedById")]
        public virtual AppUser? AddedBy { get; set; }
    }
}