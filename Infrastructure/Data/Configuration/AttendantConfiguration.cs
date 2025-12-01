using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration
{
    public class AttendantConfiguration : IEntityTypeConfiguration<Attendant>
    {
        public void Configure(EntityTypeBuilder<Attendant> builder)
        {
            builder.ToTable("attendant");
            builder.HasKey(a => a.EmployeeId); // PK is EmployeeId

            // Map properties
            builder.Property(a => a.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);

            // 1-to-1 with CrewMember
            builder.HasOne(a => a.CrewMember)
                   .WithOne(cm => cm.Attendant)
                   .HasForeignKey<Attendant>(a => a.EmployeeId)
                   .OnDelete(DeleteBehavior.NoAction);
             
            builder.Property(a => a.AppUserId).HasColumnName("AppUserId").IsRequired();
            builder.HasIndex(a => a.AppUserId).IsUnique(); // Enforce 1-to-1
            // The relationship itself is configured in AppUserConfiguration
             
            builder.Property(a => a.AddedById).IsRequired(false);
            builder.HasOne(a => a.AddedBy)
                   .WithMany()
                   .HasForeignKey(a => a.AddedById)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}