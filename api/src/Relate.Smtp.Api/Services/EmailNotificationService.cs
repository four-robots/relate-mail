using Microsoft.AspNetCore.SignalR;
using Relate.Smtp.Api.Hubs;
using Relate.Smtp.Core.Entities;
using Relate.Smtp.Infrastructure.Services;

namespace Relate.Smtp.Api.Services;

/// <summary>
/// SignalR implementation of email notification service.
/// </summary>
public class SignalREmailNotificationService : IEmailNotificationService
{
    private readonly IHubContext<EmailHub> _hubContext;

    public SignalREmailNotificationService(IHubContext<EmailHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyNewEmailAsync(Guid userId, Email email, CancellationToken ct = default)
    {
        var emailData = new
        {
            id = email.Id,
            from = email.FromAddress,
            fromDisplay = email.FromDisplayName,
            subject = email.Subject,
            receivedAt = email.ReceivedAt,
            hasAttachments = email.HasAttachments
        };

        await _hubContext.Clients
            .Group($"user_{userId}")
            .SendAsync("NewEmail", emailData, ct);
    }

    public async Task NotifyEmailUpdatedAsync(Guid userId, Guid emailId, bool isRead, CancellationToken ct = default)
    {
        var updateData = new
        {
            id = emailId,
            isRead = isRead
        };

        await _hubContext.Clients
            .Group($"user_{userId}")
            .SendAsync("EmailUpdated", updateData, ct);
    }

    public async Task NotifyEmailDeletedAsync(Guid userId, Guid emailId, CancellationToken ct = default)
    {
        await _hubContext.Clients
            .Group($"user_{userId}")
            .SendAsync("EmailDeleted", emailId, ct);
    }

    public async Task NotifyUnreadCountChangedAsync(Guid userId, int unreadCount, CancellationToken ct = default)
    {
        await _hubContext.Clients
            .Group($"user_{userId}")
            .SendAsync("UnreadCountChanged", unreadCount, ct);
    }

    public async Task NotifyMultipleUsersNewEmailAsync(IEnumerable<Guid> userIds, Email email, CancellationToken ct = default)
    {
        var emailData = new
        {
            id = email.Id,
            from = email.FromAddress,
            fromDisplay = email.FromDisplayName,
            subject = email.Subject,
            receivedAt = email.ReceivedAt,
            hasAttachments = email.HasAttachments
        };

        var tasks = userIds.Select(userId =>
            _hubContext.Clients
                .Group($"user_{userId}")
                .SendAsync("NewEmail", emailData, ct));

        await Task.WhenAll(tasks);
    }
}
