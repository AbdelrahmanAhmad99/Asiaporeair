using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration
{
    public class AdminConfiguration : IEntityTypeConfiguration<Admin>
    {
        public void Configure(EntityTypeBuilder<Admin> builder)
        {
            builder.ToTable("Admins");
            builder.HasKey(a => a.AppUserId);  
 
            builder.Property(a => a.EmployeeId).IsRequired();
            builder.HasIndex(a => a.EmployeeId).IsUnique(); 
            builder.HasOne(a => a.Employee)
                   .WithOne(e => e.Admin)
                   .HasForeignKey<Admin>(a => a.EmployeeId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.NoAction); // Prevent cascade path issues
             
            builder.Property(a => a.AddedById).IsRequired(false);
            builder.HasOne(a => a.AddedBy)
                   .WithMany() // An AppUser (SuperAdmin) can add many Admins
                   .HasForeignKey(a => a.AddedById); 
        }
    }
}