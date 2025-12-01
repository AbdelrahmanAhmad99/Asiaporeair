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
    public class AncillaryProductConfiguration : IEntityTypeConfiguration<AncillaryProduct>
    {
        public void Configure(EntityTypeBuilder<AncillaryProduct> builder)
        {
            builder.ToTable("ancillary_product");
            builder.HasKey(ap => ap.ProductId);
            builder.Property(ap => ap.ProductId).HasColumnName("product_id").UseIdentityColumn();

            builder.Property(ap => ap.Name).HasColumnName("name").HasMaxLength(50).IsRequired();
            builder.Property(ap => ap.Category).HasColumnName("category").HasMaxLength(20);
            builder.Property(ap => ap.BaseCost).HasColumnName("base_cost").HasColumnType("decimal(10,2)");
            builder.Property(ap => ap.UnitOfMeasure).HasColumnName("unit_of_measure").HasMaxLength(10);
            builder.Property(ap => ap.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);
        }
    }
}
