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
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("user");

            builder.HasKey(u => u.UserId);
             
            builder.HasIndex(u => u.AppUserId).IsUnique();
            builder.Property(ca => ca.FrequentFlyerId).HasColumnName("frequent_flyer_fk");
            builder.Property(ca => ca.KrisFlyerTier).HasColumnName("kris_flyer_tier").HasMaxLength(50);


             
            builder.HasOne(ca => ca.FrequentFlyer)
                   .WithMany(ff => ff.User)
                   .HasForeignKey(ca => ca.FrequentFlyerId)
                   .OnDelete(DeleteBehavior.NoAction);
 
            builder.HasMany(ca => ca.Bookings)
                   .WithOne(b => b.User)
                   .HasForeignKey(b => b.UserId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(ca => ca.Passengers)
                   .WithOne(p => p.User)
                   .HasForeignKey(p => p.UserId)
                   .OnDelete(DeleteBehavior.NoAction);

        }
    }
}


 