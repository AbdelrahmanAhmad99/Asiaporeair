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
    public class AncillarySaleConfiguration : IEntityTypeConfiguration<AncillarySale>
    {
        public void Configure(EntityTypeBuilder<AncillarySale> builder)
        {
            builder.ToTable("ancillary_sale");
            builder.HasKey(asl => asl.SaleId);
            builder.Property(asl => asl.SaleId).HasColumnName("sale_id").UseIdentityColumn();

            builder.Property(asl => asl.BookingId).HasColumnName("booking_fk").IsRequired();
            builder.Property(asl => asl.ProductId).HasColumnName("product_fk").IsRequired();
            builder.Property(asl => asl.Quantity).HasColumnName("quantity");
            builder.Property(asl => asl.PricePaid).HasColumnName("price_paid").HasColumnType("decimal(10,2)");
            builder.Property(asl => asl.SegmentId).HasColumnName("segment_fk");
            builder.Property(asl => asl.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);

            builder.HasOne(asl => asl.Booking)
                   .WithMany(b => b.AncillarySales)
                   .HasForeignKey(asl => asl.BookingId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(asl => asl.Product)
                   .WithMany(ap => ap.AncillarySales)
                   .HasForeignKey(asl => asl.ProductId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(asl => asl.Segment)
                   .WithMany(fl => fl.AncillarySales)
                   .HasForeignKey(asl => asl.SegmentId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}