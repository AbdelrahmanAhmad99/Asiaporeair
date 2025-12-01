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
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("payment");
            builder.HasKey(p => p.PaymentId);
            builder.Property(p => p.PaymentId).HasColumnName("payment_id").UseIdentityColumn();

            builder.Property(p => p.BookingId).HasColumnName("booking_fk").IsRequired();
            builder.Property(p => p.Amount).HasColumnName("amount").HasColumnType("decimal(10,2)").IsRequired();
            builder.Property(p => p.Method).HasColumnName("method").HasMaxLength(20).IsRequired();
            builder.Property(p => p.TransactionDateTime).HasColumnName("transaction_datetime").IsRequired();
            builder.Property(p => p.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);

            builder.HasOne(p => p.Booking)
                   .WithMany(b => b.Payments)
                   .HasForeignKey(p => p.BookingId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}