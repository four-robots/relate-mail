using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relate.Smtp.Core.Entities;

namespace Relate.Smtp.Infrastructure.Data.Configurations;

public class EmailAttachmentConfiguration : IEntityTypeConfiguration<EmailAttachment>
{
    public void Configure(EntityTypeBuilder<EmailAttachment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.FileName)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(a => a.ContentType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Content)
            .IsRequired();
    }
}
