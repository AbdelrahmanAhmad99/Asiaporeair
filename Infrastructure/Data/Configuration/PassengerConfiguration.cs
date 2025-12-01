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
    public class PassengerConfiguration : IEntityTypeConfiguration<Passenger>
    {
        public void Configure(EntityTypeBuilder<Passenger> builder)
        {
            builder.ToTable("passenger");
            builder.HasKey(p => p.PassengerId);
            builder.Property(p => p.PassengerId).HasColumnName("passenger_id").UseIdentityColumn();

            builder.Property(p => p.UserId).HasColumnName("User_fk").IsRequired();
            builder.Property(p => p.FirstName).HasColumnName("first_name").HasMaxLength(50).IsRequired();
            builder.Property(p => p.LastName).HasColumnName("last_name").HasMaxLength(50).IsRequired();
            builder.Property(p => p.DateOfBirth).HasColumnName("date_of_birth");
            builder.Property(p => p.PassportNumber).HasColumnName("passport_number").HasMaxLength(20);
            builder.Property(p => p.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);

            builder.HasOne(p => p.User)
                   .WithMany(ca => ca.Passengers)
                   .HasForeignKey(p => p.UserId)
                   .OnDelete(DeleteBehavior.NoAction);

        }
    }
}