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
    public class RouteConfiguration : IEntityTypeConfiguration<Route>
    {
        public void Configure(EntityTypeBuilder<Route> builder)
        {
            builder.ToTable("route");
            builder.HasKey(r => r.RouteId);
            builder.Property(r => r.RouteId).HasColumnName("route_id").UseIdentityColumn();

            builder.Property(r => r.OriginAirportId).HasColumnName("origin_airport_fk").HasMaxLength(3).IsRequired();
            builder.Property(r => r.DestinationAirportId).HasColumnName("destination_airport_fk").HasMaxLength(3).IsRequired();
            builder.Property(r => r.DistanceKm).HasColumnName("distance_km");
            builder.Property(r => r.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);

            builder.HasOne(r => r.OriginAirport)
                   .WithMany(a => a.OriginRoutes)
                   .HasForeignKey(r => r.OriginAirportId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(r => r.DestinationAirport)
                   .WithMany(a => a.DestinationRoutes)
                   .HasForeignKey(r => r.DestinationAirportId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}