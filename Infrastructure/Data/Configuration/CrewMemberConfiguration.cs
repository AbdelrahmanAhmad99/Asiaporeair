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
    public class CrewMemberConfiguration : IEntityTypeConfiguration<CrewMember>
    {
        public void Configure(EntityTypeBuilder<CrewMember> builder)
        {
            builder.ToTable("crew_member");
            builder.HasKey(cm => cm.EmployeeId);
            builder.Property(cm => cm.EmployeeId).HasColumnName("employee_id").IsRequired();

            builder.Property(cm => cm.CrewBaseAirportId).HasColumnName("crew_base_airport_fk").HasMaxLength(3).IsRequired();
            builder.Property(cm => cm.Position).HasColumnName("position").HasMaxLength(50);
            builder.Property(cm => cm.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);

            builder.HasOne(cm => cm.Employee)
                   .WithOne(e => e.CrewMember)
                   .HasForeignKey<CrewMember>(cm => cm.EmployeeId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(cm => cm.CrewBaseAirport)
                   .WithMany(a => a.CrewMembers)
                   .HasForeignKey(cm => cm.CrewBaseAirportId)
                   .OnDelete(DeleteBehavior.NoAction);

             
            builder.HasOne(cm => cm.Pilot)
                   .WithOne(p => p.CrewMember)
                   .HasForeignKey<Pilot>(p => p.EmployeeId);
             
            builder.HasOne(cm => cm.Attendant)
                   .WithOne(a => a.CrewMember)
                   .HasForeignKey<Attendant>(a => a.EmployeeId);

        }
    }
}