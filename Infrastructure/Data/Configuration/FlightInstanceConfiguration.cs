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
    public class FlightInstanceConfiguration : IEntityTypeConfiguration<FlightInstance>
    {
        public void Configure(EntityTypeBuilder<FlightInstance> builder)
        {
            builder.ToTable("flight_instance");
            builder.HasKey(fi => fi.InstanceId);
            builder.Property(fi => fi.InstanceId).HasColumnName("instance_id").UseIdentityColumn();

            builder.Property(fi => fi.ScheduleId).HasColumnName("schedule_fk").IsRequired();
            builder.Property(fi => fi.AircraftId).HasColumnName("aircraft_fk").HasMaxLength(10).IsRequired(false); //.IsRequired();
            builder.Property(fi => fi.ScheduledDeparture).HasColumnName("scheduled_dep_ts").IsRequired();
            builder.Property(fi => fi.ActualDeparture).HasColumnName("actual_dep_ts");
            builder.Property(fi => fi.ScheduledArrival).HasColumnName("scheduled_arr_ts").IsRequired();
            builder.Property(fi => fi.ActualArrival).HasColumnName("actual_arr_ts");
            builder.Property(fi => fi.Status).HasColumnName("status").HasMaxLength(20);
            builder.Property(fi => fi.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);

            builder.HasOne(fi => fi.Schedule)
                   .WithMany(fs => fs.FlightInstances)
                   .HasForeignKey(fi => fi.ScheduleId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(fi => fi.Aircraft)
                   .WithMany(a => a.FlightInstances)
                   .HasForeignKey(fi => fi.AircraftId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}