using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relate.Smtp.Core.Entities;

namespace Relate.Smtp.Infrastructure.Data.Configurations;

public class EmailLabelConfiguration : IEntityTypeConfiguration<EmailLabel>
{
    public void Configure(EntityTypeBuilder<EmailLabel> builder)
    {
        builder.HasKey(el => el.Id);

        builder.Property(el => el.AssignedAt)
            .IsRequired();

        // Unique index for email-label combination
        builder.HasIndex(el => new { el.EmailId, el.LabelId })
            .IsUnique();

        // Index for querying emails by user and label
        builder.HasIndex(el => new { el.UserId, el.LabelId });

        // Relationship with Email
        builder.HasOne(el => el.Email)
            .WithMany(e => e.EmailLabels)
            .HasForeignKey(el => el.EmailId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with Label
        builder.HasOne(el => el.Label)
            .WithMany(l => l.EmailLabels)
            .HasForeignKey(el => el.LabelId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relationship with User
        builder.HasOne(el => el.User)
            .WithMany(u => u.EmailLabels)
            .HasForeignKey(el => el.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
