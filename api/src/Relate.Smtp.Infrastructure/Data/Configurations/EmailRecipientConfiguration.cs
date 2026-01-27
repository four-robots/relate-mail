using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relate.Smtp.Core.Entities;

namespace Relate.Smtp.Infrastructure.Data.Configurations;

public class EmailRecipientConfiguration : IEntityTypeConfiguration<EmailRecipient>
{
    public void Configure(EntityTypeBuilder<EmailRecipient> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Address)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(r => r.DisplayName)
            .HasMaxLength(500);

        builder.Property(r => r.Type)
            .HasConversion<string>()
            .HasMaxLength(10);

        builder.HasIndex(r => r.Address);
        builder.HasIndex(r => new { r.UserId, r.IsRead });

        builder.HasOne(r => r.User)
            .WithMany(u => u.ReceivedEmails)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
