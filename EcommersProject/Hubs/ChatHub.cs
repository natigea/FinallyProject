using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace EcommersProject.Hubs;

[Authorize]
public class ChatHub : Hub
{
    // Each user joins their own group so the server can push to a specific user
    // regardless of how many connections they have open.
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (userId != null)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        await base.OnConnectedAsync();
    }
}
