using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relate.Smtp.Core.Entities;

namespace Relate.Smtp.Infrastructure.Data.Configurations;

public class LabelConfiguration : IEntityTypeConfiguration<Label>
{
    public void Configure(EntityTypeBuilder<Label> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.Color)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(l => l.CreatedAt)
            .IsRequired();

        // Index for unique label name per user
        builder.HasIndex(l => new { l.UserId, l.Name })
            .IsUnique();

        // Relationship with User
        builder.HasOne(l => l.User)
            .WithMany(u => u.Labels)
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
