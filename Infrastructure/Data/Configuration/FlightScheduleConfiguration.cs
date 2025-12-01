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
    public class FlightScheduleConfiguration : IEntityTypeConfiguration<FlightSchedule>
    {
        public void Configure(EntityTypeBuilder<FlightSchedule> builder)
        {
            builder.ToTable("flight_schedule");
            builder.HasKey(fs => fs.ScheduleId);
            builder.Property(fs => fs.ScheduleId).HasColumnName("schedule_id").UseIdentityColumn();

            builder.Property(fs => fs.FlightNo).HasColumnName("flight_no").HasMaxLength(10).IsRequired();
            builder.Property(fs => fs.RouteId).HasColumnName("route_fk").IsRequired();
            builder.Property(fs => fs.AirlineId).HasColumnName("airline_fk").HasMaxLength(2).IsRequired();
            builder.Property(fs => fs.AircraftTypeId).HasColumnName("aircraft_type_fk").IsRequired();
            builder.Property(fs => fs.DepartureTimeScheduled).HasColumnName("departure_time_scheduled").IsRequired();
            builder.Property(fs => fs.ArrivalTimeScheduled).HasColumnName("arrival_time_scheduled").IsRequired();
            builder.Property(fs => fs.DaysOfWeek).HasColumnName("days_of_week");
            builder.Property(fs => fs.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);

            builder.HasOne(fs => fs.Route)
                   .WithMany(r => r.FlightSchedules)
                   .HasForeignKey(fs => fs.RouteId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(fs => fs.Airline)
                   .WithMany(al => al.FlightSchedules)
                   .HasForeignKey(fs => fs.AirlineId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(fs => fs.AircraftType)
                   .WithMany(at => at.FlightSchedules)
                   .HasForeignKey(fs => fs.AircraftTypeId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}