using Relate.Smtp.Core.Entities;
using Relate.Smtp.Core.Interfaces;

namespace Relate.Smtp.Infrastructure.Services;

/// <summary>
/// Interface for sending email notifications.
/// Implementations can use SignalR, webhooks, or other mechanisms.
/// </summary>
public interface IEmailNotificationService
{
    Task NotifyNewEmailAsync(Guid userId, Email email, CancellationToken ct = default);
    Task NotifyMultipleUsersNewEmailAsync(IEnumerable<Guid> userIds, Email email, CancellationToken ct = default);
    Task NotifyEmailUpdatedAsync(Guid userId, Guid emailId, bool isRead, CancellationToken ct = default);
    Task NotifyEmailDeletedAsync(Guid userId, Guid emailId, CancellationToken ct = default);
    Task NotifyUnreadCountChangedAsync(Guid userId, int unreadCount, CancellationToken ct = default);
}

/// <summary>
/// No-op implementation for when SignalR is not available.
/// </summary>
public class NoOpEmailNotificationService : IEmailNotificationService
{
    public Task NotifyNewEmailAsync(Guid userId, Email email, CancellationToken ct = default) => Task.CompletedTask;
    public Task NotifyMultipleUsersNewEmailAsync(IEnumerable<Guid> userIds, Email email, CancellationToken ct = default) => Task.CompletedTask;
    public Task NotifyEmailUpdatedAsync(Guid userId, Guid emailId, bool isRead, CancellationToken ct = default) => Task.CompletedTask;
    public Task NotifyEmailDeletedAsync(Guid userId, Guid emailId, CancellationToken ct = default) => Task.CompletedTask;
    public Task NotifyUnreadCountChangedAsync(Guid userId, int unreadCount, CancellationToken ct = default) => Task.CompletedTask;
}
