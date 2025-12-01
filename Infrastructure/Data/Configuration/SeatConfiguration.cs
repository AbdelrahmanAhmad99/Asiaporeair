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
    public class SeatConfiguration : IEntityTypeConfiguration<Seat>
    {
        public void Configure(EntityTypeBuilder<Seat> builder)
        {
            builder.ToTable("seat");
            builder.HasKey(s => s.SeatId);
            builder.Property(s => s.SeatId).HasColumnName("seat_id").HasMaxLength(20).IsRequired();

            builder.Property(s => s.AircraftId).HasColumnName("aircraft_fk").HasMaxLength(10).IsRequired();
            builder.Property(s => s.SeatNumber).HasColumnName("seat_number").HasMaxLength(10).IsRequired();
            builder.Property(s => s.CabinClassId).HasColumnName("cabin_class_fk").IsRequired();
            builder.Property(s => s.IsWindow).HasColumnName("is_window");
            builder.Property(s => s.IsExitRow).HasColumnName("is_exit_row");
            builder.Property(s => s.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);

            builder.HasOne(s => s.Aircraft)
                   .WithMany(a => a.Seats)
                   .HasForeignKey(s => s.AircraftId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(s => s.CabinClass)
                   .WithMany(cc => cc.Seats)
                   .HasForeignKey(s => s.CabinClassId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}