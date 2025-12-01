using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configuration
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("employee");
            builder.HasKey(e => e.EmployeeId);

            // Explicit column mapping to match SQL
            builder.Property(e => e.DateOfHire).HasColumnName("date_of_hire").HasColumnType("DATE");
            builder.Property(e => e.Salary).HasColumnName("salary").HasColumnType("decimal(10,2)");
            builder.Property(e => e.ShiftPreferenceFk).HasColumnName("shift_preference_fk");
            builder.Property(e => e.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);

            // 1-to-1 with AppUser (Employee has the FK)
            builder.Property(e => e.AppUserId).HasColumnName("AppUserId").IsRequired();
            builder.HasIndex(e => e.AppUserId).IsUnique(); // Enforce 1-to-1

            // 1-to-1 with CrewMember (CrewMember has the FK)
            builder.HasOne(e => e.CrewMember)
                   .WithOne(cm => cm.Employee)
                   .HasForeignKey<CrewMember>(cm => cm.EmployeeId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}