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
    public class BookingPassengerConfiguration : IEntityTypeConfiguration<BookingPassenger>
    {
        public void Configure(EntityTypeBuilder<BookingPassenger> builder)
        {
            builder.ToTable("booking_passenger");
            builder.HasKey(bp => new { bp.BookingId, bp.PassengerId });

            builder.Property(bp => bp.BookingId).HasColumnName("booking_id").IsRequired();
            builder.Property(bp => bp.PassengerId).HasColumnName("passenger_id").IsRequired();
            builder.Property(bp => bp.SeatAssignmentId).HasColumnName("seat_assignment_fk").HasMaxLength(20);
            builder.Property(bp => bp.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);

            builder.HasOne(bp => bp.Booking)
                   .WithMany(b => b.BookingPassengers)
                   .HasForeignKey(bp => bp.BookingId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(bp => bp.Passenger)
                   .WithMany(p => p.BookingPassengers)
                   .HasForeignKey(bp => bp.PassengerId)
                   .OnDelete(DeleteBehavior.NoAction);

            //1. Changing the relationship from WithOne to WithMany
            builder.HasOne(bp => bp.SeatAssignment)
                   .WithMany(s => s.BookingPassengers) //The seat is associated with a large number of bookings.
                   .HasForeignKey(bp => bp.SeatAssignmentId)
                   .OnDelete(DeleteBehavior.NoAction);

            // 2. Explicitly define the index to be non-unique.
            // This will delete the old unique index and create a new one that allows duplication.
            builder.HasIndex(bp => bp.SeatAssignmentId)
                   .IsUnique(false)
                   .HasDatabaseName("IX_booking_passenger_seat_assignment_fk");


        }
    }
}