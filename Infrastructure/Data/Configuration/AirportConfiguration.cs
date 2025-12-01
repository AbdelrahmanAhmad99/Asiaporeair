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
    public class AirportConfiguration : IEntityTypeConfiguration<Airport>
    {
        public void Configure(EntityTypeBuilder<Airport> builder)
        {
            builder.ToTable("airport");
            builder.HasKey(a => a.IataCode);
            builder.Property(a => a.IataCode).HasColumnName("iata_code").HasMaxLength(3).IsRequired();

            builder.HasIndex(a => a.IcaoCode).IsUnique();
            builder.Property(a => a.IcaoCode).HasColumnName("icao_code").HasMaxLength(4).IsRequired();
            builder.Property(a => a.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            builder.Property(a => a.City).HasColumnName("city").HasMaxLength(100).IsRequired();
            builder.Property(a => a.CountryId).HasColumnName("country_fk").HasMaxLength(3).IsRequired();
            builder.Property(a => a.Latitude).HasColumnName("latitude").HasColumnType("decimal(9,6)").IsRequired();
            builder.Property(a => a.Longitude).HasColumnName("longitude").HasColumnType("decimal(9,6)").IsRequired();
            builder.Property(a => a.Altitude).HasColumnName("altitude");
            builder.Property(a => a.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);

            builder.HasOne(a => a.Country)
                   .WithMany(c => c.Airports)
                   .HasForeignKey(a => a.CountryId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}