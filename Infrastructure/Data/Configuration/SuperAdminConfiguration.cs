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
    public class SuperAdminConfiguration : IEntityTypeConfiguration<SuperAdmin>
    {
        public void Configure(EntityTypeBuilder<SuperAdmin> builder)
        {

            // 1-to-1 relationship with Employee
            builder.Property(a => a.EmployeeId).IsRequired();
            builder.HasIndex(a => a.EmployeeId).IsUnique(); // EmployeeId must be unique
            builder.HasOne(a => a.Employee)
                   .WithOne(e => e.SuperAdmin)
                   .HasForeignKey<SuperAdmin>(a => a.EmployeeId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.NoAction); // Prevent cascade path issues

            // The primary key is AppUserId to ensure a strong 1-to-1 relationship with AppUser
            builder.HasKey(a => a.AppUserId);

        }
    }
}