namespace Relate.Smtp.Api.Models;

public record SmtpApiKeyDto(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt,
    DateTimeOffset? LastUsedAt,
    bool IsActive
);

public record CreateSmtpApiKeyRequest(string Name);

public record CreatedSmtpApiKeyDto(
    Guid Id,
    string Name,
    string ApiKey,
    DateTimeOffset CreatedAt
);

public record SmtpConnectionInfoDto(
    string Server,
    int Port,
    int SecurePort,
    string Username,
    int ActiveKeyCount
);

public record SmtpCredentialsDto(
    SmtpConnectionInfoDto ConnectionInfo,
    IReadOnlyList<SmtpApiKeyDto> Keys
);
