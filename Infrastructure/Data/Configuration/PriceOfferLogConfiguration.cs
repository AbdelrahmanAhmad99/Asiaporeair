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
    public class PriceOfferLogConfiguration : IEntityTypeConfiguration<PriceOfferLog>
    {
        public void Configure(EntityTypeBuilder<PriceOfferLog> builder)
        {
            builder.ToTable("price_offer_log");
            builder.HasKey(pol => pol.OfferId);
            builder.Property(pol => pol.OfferId).HasColumnName("offer_id").UseIdentityColumn();

            builder.Property(pol => pol.ProductId).HasColumnName("product_fk");
            builder.Property(pol => pol.OfferPriceQuote).HasColumnName("offer_price_quote").HasColumnType("decimal(10,2)").IsRequired();
            builder.Property(pol => pol.Timestamp).HasColumnName("timestamp").IsRequired();
            builder.Property(pol => pol.ContextAttributesId).HasColumnName("context_attributes_fk").IsRequired();
            builder.Property(pol => pol.FareId).HasColumnName("fare_fk").HasMaxLength(10);
            builder.Property(pol => pol.AncillaryId).HasColumnName("ancillary_fk");
            builder.Property(pol => pol.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);

            builder.HasOne(pol => pol.ContextAttributes)
                   .WithMany(cpa => cpa.PriceOfferLogs)
                   .HasForeignKey(pol => pol.ContextAttributesId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(pol => pol.Fare)
                   .WithMany(fbc => fbc.PriceOfferLogs)
                   .HasForeignKey(pol => pol.FareId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(pol => pol.Ancillary)
                   .WithMany(ap => ap.PriceOfferLogs)
                   .HasForeignKey(pol => pol.AncillaryId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}