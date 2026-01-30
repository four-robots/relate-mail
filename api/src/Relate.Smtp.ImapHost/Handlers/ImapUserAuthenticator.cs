using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Relate.Smtp.Core.Interfaces;
using System.Collections.Concurrent;

namespace Relate.Smtp.ImapHost.Handlers;

public class ImapUserAuthenticator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ImapUserAuthenticator> _logger;
    private readonly ConcurrentDictionary<string, CacheEntry> _authCache = new();
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(30);

    public ImapUserAuthenticator(IServiceProvider serviceProvider, ILogger<ImapUserAuthenticator> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<(bool IsAuthenticated, Guid? UserId)> AuthenticateAsync(
        string username,
        string password,
        CancellationToken ct)
    {
        var normalizedEmail = username.ToLowerInvariant();
        var cacheKey = $"{normalizedEmail}:{password}";

        // Check cache (30-second TTL)
        if (_authCache.TryGetValue(cacheKey, out var cached) &&
            DateTimeOffset.UtcNow < cached.ExpiresAt)
        {
            _logger.LogDebug("IMAP authentication cache hit for: {Email}", username);
            _ = UpdateLastUsedAsync(cached.KeyId); // Background update
            return (cached.IsAuthenticated, cached.UserId);
        }

        // Create scoped container for DB access
        using var scope = _serviceProvider.CreateScope();
        var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var apiKeyRepo = scope.ServiceProvider.GetRequiredService<ISmtpApiKeyRepository>();

        var user = await userRepo.GetByEmailWithApiKeysAsync(normalizedEmail, ct);
        if (user == null)
        {
            _logger.LogWarning("IMAP authentication failed: User not found: {Email}", username);
            CacheResult(cacheKey, false, null, Guid.Empty);
            return (false, null);
        }

        // Verify password against each active API key using BCrypt
        foreach (var apiKey in user.SmtpApiKeys)
        {
            if (BCrypt.Net.BCrypt.Verify(password, apiKey.KeyHash))
            {
                // Check if key has imap scope
                if (!apiKeyRepo.HasScope(apiKey, "imap"))
                {
                    _logger.LogWarning("IMAP authentication failed for {Email} - API key {KeyName} lacks 'imap' scope", username, apiKey.Name);
                    CacheResult(cacheKey, false, null, Guid.Empty);
                    return (false, null);
                }

                _logger.LogInformation("IMAP user authenticated: {Email} using key: {KeyName}", username, apiKey.Name);
                CacheResult(cacheKey, true, user.Id, apiKey.Id);
                _ = UpdateLastUsedAsync(apiKey.Id); // Background update
                return (true, user.Id);
            }
        }

        _logger.LogWarning("IMAP authentication failed: Invalid API key for user: {Email}", username);
        CacheResult(cacheKey, false, null, Guid.Empty);
        return (false, null);
    }

    private void CacheResult(string key, bool authenticated, Guid? userId, Guid keyId)
    {
        _authCache[key] = new CacheEntry
        {
            IsAuthenticated = authenticated,
            UserId = userId,
            KeyId = keyId,
            ExpiresAt = DateTimeOffset.UtcNow.Add(CacheDuration)
        };

        // Cleanup if cache grows too large
        if (_authCache.Count > 1000)
        {
            var expired = _authCache
                .Where(x => DateTimeOffset.UtcNow >= x.Value.ExpiresAt)
                .Select(x => x.Key)
                .ToList();
            foreach (var k in expired)
                _authCache.TryRemove(k, out _);
        }
    }

    private Task UpdateLastUsedAsync(Guid keyId)
    {
        // Background task to update LastUsedAt
        return Task.Run(async () =>
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var apiKeyRepo = scope.ServiceProvider.GetRequiredService<ISmtpApiKeyRepository>();
                await apiKeyRepo.UpdateLastUsedAsync(keyId, DateTimeOffset.UtcNow, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update LastUsedAt for API key: {KeyId}", keyId);
            }
        });
    }

    private record CacheEntry
    {
        public bool IsAuthenticated { get; init; }
        public Guid? UserId { get; init; }
        public Guid KeyId { get; init; }
        public DateTimeOffset ExpiresAt { get; init; }
    }
}
