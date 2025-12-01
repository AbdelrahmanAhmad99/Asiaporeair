using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration
{
    public class PilotConfiguration : IEntityTypeConfiguration<Pilot>
    {
        public void Configure(EntityTypeBuilder<Pilot> builder)
        {
            builder.ToTable("pilot");
            builder.HasKey(p => p.EmployeeId); // PK is EmployeeId

            // Map properties
            builder.Property(p => p.LicenseNumber).HasColumnName("license_number").HasMaxLength(20).IsRequired();
            builder.Property(p => p.TotalFlightHours).HasColumnName("total_flight_hours");
            builder.Property(p => p.AircraftTypeId).HasColumnName("type_rating_fk").IsRequired();
            builder.Property(p => p.LastSimCheckDate).HasColumnName("last_sim_check_date").HasColumnType("DATE");
            builder.Property(p => p.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);

            // 1-to-1 with CrewMember
            builder.HasOne(p => p.CrewMember)
                   .WithOne(cm => cm.Pilot)
                   .HasForeignKey<Pilot>(p => p.EmployeeId)
                   .OnDelete(DeleteBehavior.NoAction);

            // 1-to-Many with AircraftType
            builder.HasOne(p => p.TypeRating)
                   .WithMany() // Assuming AircraftType doesn't have a collection of Pilots
                   .HasForeignKey(p => p.AircraftTypeId)
                   .OnDelete(DeleteBehavior.NoAction);
             
            builder.Property(p => p.AppUserId).HasColumnName("AppUserId").IsRequired();
            builder.HasIndex(p => p.AppUserId).IsUnique(); // Enforce 1-to-1
            // The relationship itself is configured in AppUserConfiguration
             
            builder.Property(a => a.AddedById).IsRequired(false);
            builder.HasOne(a => a.AddedBy)
                   .WithMany()
                   .HasForeignKey(a => a.AddedById)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}