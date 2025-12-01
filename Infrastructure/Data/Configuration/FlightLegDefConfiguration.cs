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
    public class FlightLegDefConfiguration : IEntityTypeConfiguration<FlightLegDef>
    {
        public void Configure(EntityTypeBuilder<FlightLegDef> builder)
        {
            builder.ToTable("flight_leg_def");
            builder.HasKey(fld => fld.LegDefId);
            builder.Property(fld => fld.LegDefId).HasColumnName("leg_def_id").UseIdentityColumn();

            builder.Property(fld => fld.ScheduleId).HasColumnName("schedule_fk").IsRequired();
            builder.Property(fld => fld.SegmentNumber).HasColumnName("segment_number").IsRequired();
            builder.Property(fld => fld.DepartureAirportId).HasColumnName("departure_airport_fk").HasMaxLength(3).IsRequired();
            builder.Property(fld => fld.ArrivalAirportId).HasColumnName("arrival_airport_fk").HasMaxLength(3).IsRequired();
            builder.Property(fld => fld.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);

            builder.HasOne(fld => fld.Schedule)
                   .WithMany(fs => fs.FlightLegs)
                   .HasForeignKey(fld => fld.ScheduleId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(fld => fld.DepartureAirport)
                   .WithMany(a => a.DepartureLegs)
                   .HasForeignKey(fld => fld.DepartureAirportId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(fld => fld.ArrivalAirport)
                   .WithMany(a => a.ArrivalLegs)
                   .HasForeignKey(fld => fld.ArrivalAirportId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}