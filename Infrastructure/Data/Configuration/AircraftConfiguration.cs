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
    public class AircraftConfiguration : IEntityTypeConfiguration<Aircraft>
    {
        public void Configure(EntityTypeBuilder<Aircraft> builder)
        {
            builder.ToTable("aircraft");

            builder.HasKey(a => a.TailNumber);
            builder.Property(a => a.TailNumber)
                .HasColumnName("tail_number")
                .HasMaxLength(10)
                .IsRequired();

            builder.Property(a => a.AirlineId)
                .HasColumnName("airline_fk")
                .HasMaxLength(2)
                .IsRequired();

            builder.Property(a => a.AircraftTypeId)
                .HasColumnName("aircraft_type_fk")
                .IsRequired();

            builder.Property(a => a.TotalFlightHours).HasColumnName("total_flight_hours");
            builder.Property(a => a.AcquisitionDate).HasColumnName("acquisition_date");

            builder.Property(a => a.Status)
                .HasColumnName("status")
                .HasMaxLength(20);

            builder.Property(a => a.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);

            builder.HasOne(a => a.Airline)
                .WithMany(al => al.Aircrafts)
                .HasForeignKey(a => a.AirlineId);

            builder.HasOne(a => a.AircraftType)
                .WithMany(at => at.Aircrafts)
                .HasForeignKey(a => a.AircraftTypeId);
        }
    }
}