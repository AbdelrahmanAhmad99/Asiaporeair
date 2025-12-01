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
    public class BoardingPassConfiguration : IEntityTypeConfiguration<BoardingPass>
    {
        public void Configure(EntityTypeBuilder<BoardingPass> builder)
        {
            builder.ToTable("boarding_pass");
            builder.HasKey(bp => bp.PassId);
            builder.Property(bp => bp.PassId).HasColumnName("pass_id").UseIdentityColumn();

            builder.Property(bp => bp.BookingPassengerBookingId).HasColumnName("booking_passenger_booking_id").IsRequired();
            builder.Property(bp => bp.BookingPassengerPassengerId).HasColumnName("booking_passenger_passenger_id").IsRequired();
            builder.Property(bp => bp.SeatId).HasColumnName("seat_fk").HasMaxLength(20);
            builder.Property(bp => bp.BoardingTime).HasColumnName("boarding_time");
            builder.Property(bp => bp.PrecheckStatus).HasColumnName("precheck_status");
            builder.Property(bp => bp.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);

            builder.HasOne(bp => bp.BookingPassenger)
                   .WithMany(bp => bp.BoardingPasses)
                   .HasForeignKey(bp => new { bp.BookingPassengerBookingId, bp.BookingPassengerPassengerId })
                   .OnDelete(DeleteBehavior.NoAction);

            // 1. Changing the relationship from WithOne to WithMany
            builder.HasOne(bp => bp.Seat)
                   .WithMany(s => s.BoardingPasss) // The seat is associated with a large number of bookings.
                   .HasForeignKey(bp => bp.SeatId)
                   .OnDelete(DeleteBehavior.NoAction);

            // 2. Explicitly define the index to be non-unique.
            // This will delete the old unique index and create a new one that allows duplication.
            builder.HasIndex(bp => bp.SeatId)
                   .IsUnique(false);



        }
    }
}