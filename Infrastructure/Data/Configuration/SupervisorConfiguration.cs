using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration
{
    public class SupervisorConfiguration : IEntityTypeConfiguration<Supervisor>
    {
        public void Configure(EntityTypeBuilder<Supervisor> builder)
        {
            builder.ToTable("Supervisors");
            builder.HasKey(a => a.AppUserId); // PK is AppUserId

            // 1-to-1 relationship with Employee
            builder.Property(a => a.EmployeeId).IsRequired();
            builder.HasIndex(a => a.EmployeeId).IsUnique(); // EmployeeId must be unique
            builder.HasOne(a => a.Employee)
                   .WithOne(e => e.Supervisor)
                   .HasForeignKey<Supervisor>(a => a.EmployeeId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.NoAction); // Prevent cascade path issues
  
            
            builder.Property(a => a.AddedById).IsRequired(false);
            builder.HasOne(a => a.AddedBy)
                   .WithMany() // An AppUser (SuperAdmin) can add many Admins
                   .HasForeignKey(a => a.AddedById); 



        }
    }
}