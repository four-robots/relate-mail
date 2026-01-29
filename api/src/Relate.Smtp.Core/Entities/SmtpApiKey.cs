namespace Relate.Smtp.Core.Entities;

public class SmtpApiKey
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string KeyHash { get; set; } = string.Empty;

    /// <summary>
    /// JSON array of permission scopes, e.g., ["smtp", "pop3", "api:read", "api:write"]
    /// </summary>
    public string Scopes { get; set; } = "[]";

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastUsedAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }

    public User User { get; set; } = null!;
}
