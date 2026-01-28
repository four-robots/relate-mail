namespace Relate.Smtp.Pop3Host.Protocol;

public class Pop3Session
{
    public string ConnectionId { get; init; } = Guid.NewGuid().ToString();
    public DateTime ConnectedAt { get; init; } = DateTime.UtcNow;
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;

    public Pop3State State { get; set; } = Pop3State.Authorization;
    public string? Username { get; set; }
    public Guid? UserId { get; set; }

    public List<Pop3Message> Messages { get; set; } = new();
    public HashSet<int> DeletedMessages { get; set; } = new();

    public bool IsTimedOut(TimeSpan timeout) =>
        DateTime.UtcNow - LastActivityAt > timeout;
}

public class Pop3Message
{
    public int MessageNumber { get; set; }  // 1-based
    public Guid EmailId { get; set; }
    public long SizeBytes { get; set; }
    public string UniqueId { get; set; } = string.Empty;
}
