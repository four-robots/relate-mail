using Microsoft.AspNetCore.SignalR;

namespace Relate.Smtp.Api.Hubs;

/// <summary>
/// SignalR hub for real-time email notifications.
/// Clients connect and are automatically added to a group named after their user ID.
/// </summary>
public class EmailHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // Get user ID from claims (set by JWT authentication)
        var userId = Context.User?.FindFirst("sub")?.Value
                     ?? Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            // Add connection to user-specific group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst("sub")?.Value
                     ?? Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }
}
