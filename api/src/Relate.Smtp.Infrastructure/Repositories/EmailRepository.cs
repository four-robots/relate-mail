using Microsoft.EntityFrameworkCore;
using Relate.Smtp.Core.Entities;
using Relate.Smtp.Core.Interfaces;
using Relate.Smtp.Infrastructure.Data;

namespace Relate.Smtp.Infrastructure.Repositories;

public class EmailRepository : IEmailRepository
{
    private readonly AppDbContext _context;

    public EmailRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Email?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Emails
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<Email?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Emails
            .Include(e => e.Recipients)
            .Include(e => e.Attachments)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Email>> GetByUserIdAsync(Guid userId, int skip, int take, CancellationToken cancellationToken = default)
    {
        return await _context.Emails
            .Include(e => e.Recipients)
            .Where(e => e.Recipients.Any(r => r.UserId == userId))
            .OrderByDescending(e => e.ReceivedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Emails
            .Where(e => e.Recipients.Any(r => r.UserId == userId))
            .CountAsync(cancellationToken);
    }

    public async Task<int> GetUnreadCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.EmailRecipients
            .Where(r => r.UserId == userId && !r.IsRead)
            .CountAsync(cancellationToken);
    }

    public async Task<Email> AddAsync(Email email, CancellationToken cancellationToken = default)
    {
        _context.Emails.Add(email);
        await _context.SaveChangesAsync(cancellationToken);
        return email;
    }

    public async Task UpdateAsync(Email email, CancellationToken cancellationToken = default)
    {
        _context.Emails.Update(email);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var email = await _context.Emails.FindAsync([id], cancellationToken);
        if (email != null)
        {
            _context.Emails.Remove(email);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task LinkEmailsToUserAsync(Guid userId, IEnumerable<string> emailAddresses, CancellationToken cancellationToken = default)
    {
        var addresses = emailAddresses.Select(a => a.ToLowerInvariant()).ToList();

        var recipients = await _context.EmailRecipients
            .Where(r => r.UserId == null && addresses.Contains(r.Address.ToLower()))
            .ToListAsync(cancellationToken);

        foreach (var recipient in recipients)
        {
            recipient.UserId = userId;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
