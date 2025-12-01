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
    public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        { 
            builder.ToTable("ticket");
             
            builder.HasKey(t => t.TicketId); 

            builder.Property(t => t.TicketCode)
                .IsRequired()
                .HasMaxLength(20);
             
            builder.HasIndex(t => t.TicketCode)
                .IsUnique();

            builder.Property(t => t.IssueDate)
                .IsRequired();
             
            builder.Property(t => t.Status)
                .IsRequired()
                .HasMaxLength(20)
                .HasConversion<string>();

            builder.Property(t => t.IsDeleted)
                .HasDefaultValue(false);

            // Setting up external relationships and keys
            builder.HasOne(t => t.Passenger)
                .WithMany()  
                .HasForeignKey(t => t.PassengerId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent user deletion if they have tickets

            builder.HasOne(t => t.Booking)
                .WithMany(b => b.Tickets)  
                .HasForeignKey(t => t.BookingId)
                .OnDelete(DeleteBehavior.Cascade);  

            builder.HasOne(t => t.FlightInstance)
                .WithMany()
                .HasForeignKey(t => t.FlightInstanceId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Seat)
                .WithMany()
                .HasForeignKey(t => t.SeatId)
                .IsRequired(false)  
                .OnDelete(DeleteBehavior.SetNull); // When deleting a seat, leave the value blank in the ticket.
             
            builder.HasOne(t => t.FrequentFlyer)
                .WithMany()
                .HasForeignKey(t => t.FrequentFlyerId)
                .IsRequired(false)  
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}