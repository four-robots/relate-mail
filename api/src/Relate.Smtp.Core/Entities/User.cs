namespace Relate.Smtp.Core.Entities;

public class User
{
    public Guid Id { get; set; }
    public string OidcSubject { get; set; } = string.Empty;
    public string OidcIssuer { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }

    public ICollection<UserEmailAddress> AdditionalAddresses { get; set; } = new List<UserEmailAddress>();
    public ICollection<EmailRecipient> ReceivedEmails { get; set; } = new List<EmailRecipient>();
}
