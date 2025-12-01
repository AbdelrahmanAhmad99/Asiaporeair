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
    public class RouteOperatorConfiguration : IEntityTypeConfiguration<RouteOperator>
    {
        public void Configure(EntityTypeBuilder<RouteOperator> builder)
        {
            builder.ToTable("route_operator");
            builder.HasKey(ro => new { ro.RouteId, ro.AirlineId });

            builder.Property(ro => ro.RouteId).HasColumnName("route_fk").IsRequired();
            builder.Property(ro => ro.AirlineId).HasColumnName("airline_fk").HasMaxLength(2).IsRequired();
            builder.Property(ro => ro.CodeshareStatus).HasColumnName("codeshare_status");
            builder.Property(ro => ro.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);

            builder.HasOne(ro => ro.Route)
                   .WithMany(r => r.RouteOperators)
                   .HasForeignKey(ro => ro.RouteId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(ro => ro.Airline)
                   .WithMany(al => al.RouteOperators)
                   .HasForeignKey(ro => ro.AirlineId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}