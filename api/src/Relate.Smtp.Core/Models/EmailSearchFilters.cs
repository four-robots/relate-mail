namespace Relate.Smtp.Core.Models;

/// <summary>
/// Filters for email search operations.
/// </summary>
public class EmailSearchFilters
{
    /// <summary>
    /// Full-text search query (searches From, Subject, Body).
    /// </summary>
    public string? Query { get; set; }

    /// <summary>
    /// Filter by emails received after this date.
    /// </summary>
    public DateTimeOffset? FromDate { get; set; }

    /// <summary>
    /// Filter by emails received before this date.
    /// </summary>
    public DateTimeOffset? ToDate { get; set; }

    /// <summary>
    /// Filter by emails with attachments.
    /// </summary>
    public bool? HasAttachments { get; set; }

    /// <summary>
    /// Filter by read/unread status.
    /// </summary>
    public bool? IsRead { get; set; }
}
