using Relate.Smtp.Core.Entities;

namespace Relate.Smtp.Core.Interfaces;

public interface ISmtpApiKeyRepository
{
    Task<IReadOnlyList<SmtpApiKey>> GetActiveKeysForUserAsync(Guid userId, CancellationToken ct = default);
    Task<SmtpApiKey> CreateAsync(SmtpApiKey key, CancellationToken ct = default);
    Task RevokeAsync(Guid keyId, CancellationToken ct = default);
    Task UpdateLastUsedAsync(Guid keyId, DateTimeOffset lastUsed, CancellationToken ct = default);
}
