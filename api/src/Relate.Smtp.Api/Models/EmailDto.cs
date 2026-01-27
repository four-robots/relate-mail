using Relate.Smtp.Core.Entities;

namespace Relate.Smtp.Api.Models;

public record EmailListItemDto(
    Guid Id,
    string MessageId,
    string FromAddress,
    string? FromDisplayName,
    string Subject,
    DateTimeOffset ReceivedAt,
    long SizeBytes,
    bool IsRead,
    int AttachmentCount
);

public record EmailDetailDto(
    Guid Id,
    string MessageId,
    string FromAddress,
    string? FromDisplayName,
    string Subject,
    string? TextBody,
    string? HtmlBody,
    DateTimeOffset ReceivedAt,
    long SizeBytes,
    bool IsRead,
    List<EmailRecipientDto> Recipients,
    List<EmailAttachmentDto> Attachments
);

public record EmailRecipientDto(
    Guid Id,
    string Address,
    string? DisplayName,
    string Type
);

public record EmailAttachmentDto(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeBytes
);

public record EmailListResponse(
    List<EmailListItemDto> Items,
    int TotalCount,
    int UnreadCount,
    int Page,
    int PageSize
);

public record UpdateEmailRequest(
    bool? IsRead
);

public static class EmailMappingExtensions
{
    public static EmailListItemDto ToListItemDto(this Email email, Guid userId)
    {
        var recipient = email.Recipients.FirstOrDefault(r => r.UserId == userId);
        return new EmailListItemDto(
            email.Id,
            email.MessageId,
            email.FromAddress,
            email.FromDisplayName,
            email.Subject,
            email.ReceivedAt,
            email.SizeBytes,
            recipient?.IsRead ?? false,
            email.Attachments.Count
        );
    }

    public static EmailDetailDto ToDetailDto(this Email email, Guid userId)
    {
        var recipient = email.Recipients.FirstOrDefault(r => r.UserId == userId);
        return new EmailDetailDto(
            email.Id,
            email.MessageId,
            email.FromAddress,
            email.FromDisplayName,
            email.Subject,
            email.TextBody,
            email.HtmlBody,
            email.ReceivedAt,
            email.SizeBytes,
            recipient?.IsRead ?? false,
            email.Recipients.Select(r => new EmailRecipientDto(
                r.Id,
                r.Address,
                r.DisplayName,
                r.Type.ToString()
            )).ToList(),
            email.Attachments.Select(a => new EmailAttachmentDto(
                a.Id,
                a.FileName,
                a.ContentType,
                a.SizeBytes
            )).ToList()
        );
    }
}
