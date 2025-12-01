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
    public class CountryConfiguration : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> builder)
        {
            builder.ToTable("country");
            builder.HasKey(c => c.IsoCode);
            builder.Property(c => c.IsoCode).HasColumnName("iso_code").HasMaxLength(3).IsRequired();

            builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            builder.Property(c => c.Continent).HasColumnName("continent_fk").HasMaxLength(50);
            builder.Property(c => c.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);
        }
    }
}