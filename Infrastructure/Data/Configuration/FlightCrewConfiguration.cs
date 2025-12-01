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
    public class FlightCrewConfiguration : IEntityTypeConfiguration<FlightCrew>
    {
        public void Configure(EntityTypeBuilder<FlightCrew> builder)
        {
            builder.ToTable("flight_crew");
            builder.HasKey(fc => new { fc.FlightInstanceId, fc.CrewMemberId });

            builder.Property(fc => fc.FlightInstanceId).HasColumnName("flight_instance_fk").IsRequired();
            builder.Property(fc => fc.CrewMemberId).HasColumnName("crew_member_fk").IsRequired();
            builder.Property(fc => fc.Role).HasColumnName("role").HasMaxLength(50);
            builder.Property(fc => fc.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);

            builder.HasOne(fc => fc.FlightInstance)
                   .WithMany(fi => fi.FlightCrews)
                   .HasForeignKey(fc => fc.FlightInstanceId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(fc => fc.CrewMember)
                   .WithMany(cm => cm.FlightCrews)
                   .HasForeignKey(fc => fc.CrewMemberId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}