using Microsoft.EntityFrameworkCore;
using Relate.Smtp.Core.Entities;

namespace Relate.Smtp.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Email> Emails => Set<Email>();
    public DbSet<EmailRecipient> EmailRecipients => Set<EmailRecipient>();
    public DbSet<EmailAttachment> EmailAttachments => Set<EmailAttachment>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserEmailAddress> UserEmailAddresses => Set<UserEmailAddress>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
