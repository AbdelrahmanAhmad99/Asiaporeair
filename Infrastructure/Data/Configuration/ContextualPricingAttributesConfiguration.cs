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
    public class ContextualPricingAttributesConfiguration : IEntityTypeConfiguration<ContextualPricingAttributes>
    {
        public void Configure(EntityTypeBuilder<ContextualPricingAttributes> builder)
        {
            builder.ToTable("contextual_pricing_attributes");
            builder.HasKey(cpa => cpa.AttributeId);
            builder.Property(cpa => cpa.AttributeId).HasColumnName("attribute_id").UseIdentityColumn();

            builder.Property(cpa => cpa.TimeUntilDeparture).HasColumnName("time_until_departure");
            builder.Property(cpa => cpa.LengthOfStay).HasColumnName("length_of_stay");
            builder.Property(cpa => cpa.CompetitorFares).HasColumnName("competitor_fares");
            builder.Property(cpa => cpa.WillingnessToPay).HasColumnName("willingness_to_pay").HasColumnType("decimal(10,2)");
            builder.Property(cpa => cpa.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);
        }
    }
}