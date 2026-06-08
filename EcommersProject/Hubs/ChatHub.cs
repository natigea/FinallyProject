using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace EcommersProject.Hubs;

[Authorize]
public class ChatHub(ILogger<ChatHub> logger) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier
                     ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        logger.LogInformation("[ChatHub] Connected: connId={ConnId}, userId={UserId}",
            Context.ConnectionId, userId ?? "NULL");

        if (userId != null)
        {
            var group = $"user-{userId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
            logger.LogInformation("[ChatHub] Joined group: {Group}, connId={ConnId}", group, Context.ConnectionId);
        }
        else
        {
            logger.LogWarning("[ChatHub] userId is NULL — user will NOT receive calls. Claims: {Claims}",
                string.Join(", ", Context.User?.Claims.Select(c => $"{c.Type}={c.Value}") ?? []));
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier
                     ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        logger.LogInformation("[ChatHub] Disconnected: connId={ConnId}, userId={UserId}, error={Error}",
            Context.ConnectionId, userId ?? "NULL", exception?.Message ?? "none");

        await base.OnDisconnectedAsync(exception);
    }
}
