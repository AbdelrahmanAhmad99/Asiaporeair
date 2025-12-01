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
    public class FrequentFlyerConfiguration : IEntityTypeConfiguration<FrequentFlyer>
    {
        public void Configure(EntityTypeBuilder<FrequentFlyer> builder)
        {
            builder.ToTable("frequent_flyer");
            builder.HasKey(ff => ff.FlyerId);
            builder.Property(ff => ff.FlyerId).HasColumnName("flyer_id").UseIdentityColumn();

            builder.Property(ff => ff.CardNumber).HasColumnName("card_number").HasMaxLength(50).IsRequired();
            builder.Property(ff => ff.Level).HasColumnName("level").HasMaxLength(50);
            builder.Property(ff => ff.AwardPoints).HasColumnName("award_points");
            builder.Property(ff => ff.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);


            builder.HasMany(ff => ff.User)
             .WithOne(ca => ca.FrequentFlyer)
             .HasForeignKey(ca => ca.FrequentFlyerId)
             .OnDelete(DeleteBehavior.NoAction);
        }
    }
}