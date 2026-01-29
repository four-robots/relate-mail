namespace Relate.Smtp.Core.Entities;

/// <summary>
/// Junction table linking emails to labels.
/// </summary>
public class EmailLabel
{
    public Guid Id { get; set; }
    public Guid EmailId { get; set; }
    public Guid UserId { get; set; }
    public Guid LabelId { get; set; }
    public DateTimeOffset AssignedAt { get; set; }

    // Navigation properties
    public Email? Email { get; set; }
    public User? User { get; set; }
    public Label? Label { get; set; }
}
