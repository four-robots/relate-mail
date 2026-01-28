using Microsoft.EntityFrameworkCore;
using Relate.Smtp.Core.Entities;
using Relate.Smtp.Core.Interfaces;
using Relate.Smtp.Infrastructure.Data;

namespace Relate.Smtp.Infrastructure.Repositories;

public class SmtpApiKeyRepository : ISmtpApiKeyRepository
{
    private readonly AppDbContext _context;

    public SmtpApiKeyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<SmtpApiKey>> GetActiveKeysForUserAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.SmtpApiKeys
            .Where(k => k.UserId == userId && k.RevokedAt == null)
            .OrderByDescending(k => k.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<SmtpApiKey> CreateAsync(SmtpApiKey key, CancellationToken ct = default)
    {
        _context.SmtpApiKeys.Add(key);
        await _context.SaveChangesAsync(ct);
        return key;
    }

    public async Task RevokeAsync(Guid keyId, CancellationToken ct = default)
    {
        var key = await _context.SmtpApiKeys.FindAsync([keyId], ct);
        if (key != null)
        {
            key.RevokedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task UpdateLastUsedAsync(Guid keyId, DateTimeOffset lastUsed, CancellationToken ct = default)
    {
        var key = await _context.SmtpApiKeys.FindAsync([keyId], ct);
        if (key != null)
        {
            key.LastUsedAt = lastUsed;
            await _context.SaveChangesAsync(ct);
        }
    }
}
