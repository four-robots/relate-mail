using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relate.Smtp.Core.Entities;

namespace Relate.Smtp.Infrastructure.Data.Configurations;

public class EmailFilterConfiguration : IEntityTypeConfiguration<EmailFilter>
{
    public void Configure(EntityTypeBuilder<EmailFilter> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.IsEnabled)
            .IsRequired();

        builder.Property(f => f.Priority)
            .IsRequired();

        builder.Property(f => f.FromAddressContains)
            .HasMaxLength(500);

        builder.Property(f => f.SubjectContains)
            .HasMaxLength(500);

        builder.Property(f => f.BodyContains)
            .HasMaxLength(1000);

        builder.Property(f => f.CreatedAt)
            .IsRequired();

        builder.Property(f => f.TimesApplied)
            .IsRequired()
            .HasDefaultValue(0);

        // Index for querying filters by user and priority
        builder.HasIndex(f => new { f.UserId, f.Priority });

        // Relationship with User
        builder.HasOne(f => f.User)
            .WithMany(u => u.EmailFilters)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with Label (optional)
        builder.HasOne(f => f.AssignLabel)
            .WithMany()
            .HasForeignKey(f => f.AssignLabelId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
