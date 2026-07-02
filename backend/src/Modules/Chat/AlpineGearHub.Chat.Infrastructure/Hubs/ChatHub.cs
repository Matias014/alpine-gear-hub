using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AlpineGearHub.Chat.Infrastructure.Hubs;

[Authorize]
public sealed class ChatHub : Hub
{
    // Each connection joins a group named after its user id, so a message can be pushed
    // to a user regardless of which conversation page (if any) they currently have open.
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);

        await base.OnConnectedAsync();
    }
}
