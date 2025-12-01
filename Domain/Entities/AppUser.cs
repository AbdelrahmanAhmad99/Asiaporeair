using Domain.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace Domain.Entities
{
    // This entity maps to the 'AspNetUsers' table defined in your SQL script.
    public class AppUser : IdentityUser
    { 
        [Column(TypeName = "NVARCHAR(MAX)")]
        public string FirstName { get; set; } = string.Empty;

        [Column(TypeName = "NVARCHAR(MAX)")]
        public string LastName { get; set; } = string.Empty;

        [Column(TypeName = "DATETIME2")]
        public DateTime? DateOfBirth { get; set; }

        [Column(TypeName = "NVARCHAR(MAX)")]
        public string Address { get; set; } = string.Empty;

        [Column(TypeName = "DATETIME2")]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "DATETIME2")]
        public DateTime? LastLogin { get; set; }

        public bool IsDeleted { get; set; } = false;

        // User type discriminator
        [Column(TypeName = "NVARCHAR(50)")]
        public UserType UserType { get; set; }
 
        [Column(TypeName = "NVARCHAR(MAX)")]
        public string? ProfilePictureUrl { get; set; }

        // --- Navigation properties to specialized roles ---
        // These relations are defined by the Foreign Key in the *other* tables.

        // 1-to-1 relationship with Employee (via Employee.AppUserId)
        public virtual Employee? Employee { get; set; }

        // 1-to-1 relationship with User (Passenger Profile) (via User.AppUserId)
        public virtual User? User { get; set; }

        // 1-to-1 relationship with Admin (via Admin.AppUserId)
        public virtual Admin? Admin { get; set; }

        // 1-to-1 relationship with SuperAdmin (via SuperAdmin.AppUserId)
        public virtual SuperAdmin? SuperAdmin { get; set; }

        // 1-to-1 relationship with Supervisor (via Supervisor.AppUserId)
        public virtual Supervisor? Supervisor { get; set; }

        // 1-to-1 relationship with Pilot (via Pilot.AppUserId)
        public virtual Pilot? Pilot { get; set; }

        // 1-to-1 relationship with Attendant (via Attendant.AppUserId)
        public virtual Attendant? Attendant { get; set; }
    }
}