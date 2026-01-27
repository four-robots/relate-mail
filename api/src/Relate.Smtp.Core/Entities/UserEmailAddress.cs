namespace Relate.Smtp.Core.Entities;

public class UserEmailAddress
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Address { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public DateTimeOffset AddedAt { get; set; }

    public User User { get; set; } = null!;
}
