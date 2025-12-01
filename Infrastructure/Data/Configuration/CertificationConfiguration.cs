
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
    public class CertificationConfiguration : IEntityTypeConfiguration<Certification>
    {
        public void Configure(EntityTypeBuilder<Certification> builder)
        {
            builder.ToTable("certification");
            builder.HasKey(c => c.CertId);
            builder.Property(c => c.CertId).HasColumnName("cert_id").UseIdentityColumn();

            builder.Property(c => c.CrewMemberId).HasColumnName("crew_member_fk").IsRequired();
            builder.Property(c => c.Type).HasColumnName("type").HasMaxLength(50).IsRequired();
            builder.Property(c => c.IssueDate).HasColumnName("issue_date");
            builder.Property(c => c.ExpiryDate).HasColumnName("expiry_date");
            builder.Property(c => c.IsDeleted).HasColumnName("IsDeleted").HasDefaultValue(false);

            builder.HasOne(c => c.CrewMember)
                   .WithMany(cm => cm.Certifications)
                   .HasForeignKey(c => c.CrewMemberId)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}