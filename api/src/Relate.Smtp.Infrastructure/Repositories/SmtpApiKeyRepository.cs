using System.Text.Json;
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

    public async Task<SmtpApiKey?> GetByKeyWithScopeAsync(string rawKey, string requiredScope, CancellationToken cancellationToken = default)
    {
        // Get all active keys for all users
        var activeKeys = await _context.SmtpApiKeys
            .Include(k => k.User)
            .Where(k => k.RevokedAt == null)
            .ToListAsync(cancellationToken);

        // Check each key's hash and scopes
        foreach (var key in activeKeys)
        {
            if (BCrypt.Net.BCrypt.Verify(rawKey, key.KeyHash))
            {
                // Verify scope
                if (HasScope(key, requiredScope))
                {
                    return key;
                }
                return null; // Key found but lacks scope
            }
        }

        return null; // Key not found
    }

    public IReadOnlyList<string> ParseScopes(string scopesJson)
    {
        if (string.IsNullOrWhiteSpace(scopesJson) || scopesJson == "[]")
        {
            return Array.Empty<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(scopesJson) ?? new List<string>();
        }
        catch
        {
            return Array.Empty<string>();
        }
    }

    public bool HasScope(SmtpApiKey key, string scope)
    {
        var scopes = ParseScopes(key.Scopes);
        return scopes.Contains(scope, StringComparer.OrdinalIgnoreCase);
    }
}
