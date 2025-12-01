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
    public class AircraftTypeConfiguration : IEntityTypeConfiguration<AircraftType>
    {
        public void Configure(EntityTypeBuilder<AircraftType> builder)
        {
            builder.ToTable("aircraft_type");
            builder.HasKey(at => at.TypeId);
            builder.Property(at => at.TypeId).HasColumnName("type_id").UseIdentityColumn();

            builder.Property(at => at.Model).HasColumnName("model").HasMaxLength(50).IsRequired();
            builder.Property(at => at.Manufacturer).HasColumnName("manufacturer").HasMaxLength(50).IsRequired();
            builder.Property(at => at.RangeKm).HasColumnName("range_km");
            builder.Property(at => at.MaxSeats).HasColumnName("max_seats");
            builder.Property(at => at.CargoCapacity).HasColumnName("cargo_capacity").HasColumnType("decimal(10,2)");
            builder.Property(at => at.CruisingVelocity).HasColumnName("cruising_velocity");
            builder.Property(at => at.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);
        }
    }
}