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
    public class AircraftConfigConfiguration : IEntityTypeConfiguration<AircraftConfig>
    {
        public void Configure(EntityTypeBuilder<AircraftConfig> builder)
        {
            builder.ToTable("aircraft_config");
            builder.HasKey(ac => ac.ConfigId);
            builder.Property(ac => ac.ConfigId).HasColumnName("config_id").UseIdentityColumn();

            builder.Property(ac => ac.AircraftId).HasColumnName("aircraft_fk").HasMaxLength(10).IsRequired();
            builder.Property(ac => ac.ConfigurationName).HasColumnName("configuration_name").HasMaxLength(50).IsRequired();
            builder.Property(ac => ac.TotalSeatsCount).HasColumnName("total_seats_count");
            builder.Property(ac => ac.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);

            builder.HasOne(ac => ac.Aircraft)
                   .WithMany(a => a.Configurations)
                   .HasForeignKey(ac => ac.AircraftId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}