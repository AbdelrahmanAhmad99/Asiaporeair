using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {
            // Map additional AppUser properties to the AspNetUsers table
            builder.Property(u => u.FirstName).IsRequired();
            builder.Property(u => u.LastName).IsRequired();
            builder.Property(u => u.Address).IsRequired(false);
            builder.Property(u => u.DateOfBirth).IsRequired(false);
            builder.Property(u => u.DateCreated).HasDefaultValueSql("GETDATE()");
            builder.Property(u => u.IsDeleted).HasDefaultValue(false); 
            builder.Property(u => u.ProfilePictureUrl).IsRequired(false);

            builder.Property(u => u.UserType)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);

            // --- Define 1-to-1 relationships ---
            // These are configured from the *other* side (the entity with the FK)
            builder.HasOne(au => au.Employee)
                .WithOne(e => e.AppUser)
                .HasForeignKey<Employee>(e => e.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(au => au.User)
                .WithOne(u => u.AppUser)
                .HasForeignKey<User>(u => u.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(au => au.Admin)
                .WithOne(a => a.AppUser)
                .HasForeignKey<Admin>(a => a.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(au => au.SuperAdmin)
                .WithOne(sa => sa.AppUser)
                .HasForeignKey<SuperAdmin>(sa => sa.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(au => au.Supervisor)
                .WithOne(s => s.AppUser)
                .HasForeignKey<Supervisor>(s => s.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(au => au.Pilot)
                .WithOne(p => p.AppUser)
                .HasForeignKey<Pilot>(p => p.AppUserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(au => au.Attendant)
                .WithOne(a => a.AppUser)
                .HasForeignKey<Attendant>(a => a.AppUserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}