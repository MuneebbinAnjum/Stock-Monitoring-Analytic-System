using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SMAS.API.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        // Simple hub for broadcasting inventory and alert notifications
        public async Task JoinGroup(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    await Clients.Caller.SendAsync("Error", "Invalid user ID");
                    return;
                }

                await Groups.AddToGroupAsync(Context.ConnectionId, userId);
                _logger.LogInformation("User {UserId} joined notification group", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error joining group for user {UserId}", userId);
                await Clients.Caller.SendAsync("Error", "Failed to join notification group");
            }
        }

        public async Task LeaveGroup(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                {
                    return;
                }

                await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
                _logger.LogInformation("User {UserId} left notification group", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error leaving group for user {UserId}", userId);
            }
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("SignalR client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (exception != null)
            {
                _logger.LogWarning(exception, "SignalR client disconnected abnormally: {ConnectionId}", Context.ConnectionId);
            }
            else
            {
                _logger.LogInformation("SignalR client disconnected normally: {ConnectionId}", Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}

