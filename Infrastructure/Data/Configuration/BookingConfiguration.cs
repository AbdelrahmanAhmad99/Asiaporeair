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
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.ToTable("booking");
            builder.HasKey(b => b.BookingId);
            builder.Property(b => b.BookingId).HasColumnName("booking_id").UseIdentityColumn();

            builder.Property(b => b.UserId).HasColumnName("User_fk").IsRequired();
            builder.Property(b => b.FlightInstanceId).HasColumnName("flight_instance_fk").IsRequired();
            builder.Property(b => b.BookingRef).HasColumnName("booking_ref").HasMaxLength(10).IsRequired();
            builder.Property(b => b.BookingTime).HasColumnName("booking_time").IsRequired();
            builder.Property(b => b.PriceTotal).HasColumnName("price_total").HasColumnType("decimal(10,2)");
            builder.Property(b => b.PaymentStatus).HasColumnName("payment_status").HasMaxLength(20);
            builder.Property(b => b.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);
            builder.Property(b => b.FareBasisCodeId).HasColumnName("fare_basis_code_fk").HasMaxLength(10).IsRequired();

            builder.HasOne(b => b.User)
                   .WithMany(ca => ca.Bookings)
                   .HasForeignKey(b => b.UserId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(b => b.FlightInstance)
                   .WithMany(fi => fi.Bookings)
                   .HasForeignKey(b => b.FlightInstanceId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(b => b.FareBasisCode)
                   .WithMany(fbc => fbc.Bookings)
                   .HasForeignKey(b => b.FareBasisCodeId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}