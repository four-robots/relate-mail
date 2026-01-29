namespace Relate.Smtp.Core.Entities;

/// <summary>
/// Represents a user-defined label for organizing emails.
/// </summary>
public class Label
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#3b82f6"; // Default blue
    public int SortOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public ICollection<EmailLabel> EmailLabels { get; set; } = new List<EmailLabel>();
}
