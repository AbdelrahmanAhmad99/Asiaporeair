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
    public class FareBasisCodeConfiguration : IEntityTypeConfiguration<FareBasisCode>
    {
        public void Configure(EntityTypeBuilder<FareBasisCode> builder)
        {
            builder.ToTable("fare_basis_code");
            builder.HasKey(fbc => fbc.Code);
            builder.Property(fbc => fbc.Code).HasColumnName("code").HasMaxLength(10).IsRequired();

            builder.Property(fbc => fbc.Description).HasColumnName("description");
            builder.Property(fbc => fbc.Rules).HasColumnName("rules");
            builder.Property(fbc => fbc.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);
        }
    }
}