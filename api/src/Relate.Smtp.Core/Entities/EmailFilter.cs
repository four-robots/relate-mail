namespace Relate.Smtp.Core.Entities;

/// <summary>
/// Represents a user-defined filter rule for automatically organizing emails.
/// </summary>
public class EmailFilter
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public int Priority { get; set; } // Lower number = higher priority

    // Conditions (any can be null, meaning don't filter on that field)
    public string? FromAddressContains { get; set; }
    public string? SubjectContains { get; set; }
    public string? BodyContains { get; set; }
    public bool? HasAttachments { get; set; }

    // Actions
    public bool MarkAsRead { get; set; }
    public Guid? AssignLabelId { get; set; }
    public bool Delete { get; set; }

    // Metadata
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastAppliedAt { get; set; }
    public int TimesApplied { get; set; }

    // Navigation properties
    public User? User { get; set; }
    public Label? AssignLabel { get; set; }
}
