using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Relate.Smtp.Core.Entities;

namespace Relate.Smtp.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.OidcSubject)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(u => u.OidcIssuer)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasMaxLength(320)
            .IsRequired();

        builder.Property(u => u.DisplayName)
            .HasMaxLength(500);

        builder.HasIndex(u => new { u.OidcIssuer, u.OidcSubject }).IsUnique();
        builder.HasIndex(u => u.Email);

        builder.HasMany(u => u.AdditionalAddresses)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
