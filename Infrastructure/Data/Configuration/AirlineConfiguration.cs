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
    public class AirlineConfiguration : IEntityTypeConfiguration<Airline>
    {
        public void Configure(EntityTypeBuilder<Airline> builder)
        {
            builder.ToTable("airline");
            builder.HasKey(a => a.IataCode);
            builder.Property(a => a.IataCode).HasColumnName("iata_code").HasMaxLength(2).IsRequired();

            builder.Property(a => a.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            builder.Property(a => a.Callsign).HasColumnName("callsign").HasMaxLength(50);
            builder.Property(a => a.OperatingRegion).HasColumnName("operating_region").HasMaxLength(50);
            builder.Property(a => a.BaseAirportId).HasColumnName("base_airport_fk").HasMaxLength(3).IsRequired();
            builder.Property(a => a.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);

            builder.HasOne(a => a.BaseAirport)
                   .WithMany(ap => ap.Airlines)
                   .HasForeignKey(a => a.BaseAirportId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}