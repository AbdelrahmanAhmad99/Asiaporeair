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
    public class CabinClassConfiguration : IEntityTypeConfiguration<CabinClass>
    {
        public void Configure(EntityTypeBuilder<CabinClass> builder)
        {
            builder.ToTable("cabin_class");
            builder.HasKey(cc => cc.CabinClassId);
            builder.Property(cc => cc.CabinClassId).HasColumnName("cabin_class_id").UseIdentityColumn();

            builder.Property(cc => cc.ConfigId).HasColumnName("config_fk").IsRequired();
            builder.Property(cc => cc.Name).HasColumnName("name").HasMaxLength(20).IsRequired();
            builder.Property(cc => cc.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);

            builder.HasOne(cc => cc.AircraftConfig)
                   .WithMany(ac => ac.CabinClasses)
                   .HasForeignKey(cc => cc.ConfigId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
