using AlpineGearHub.Chat.Application.DTOs;
using AlpineGearHub.Chat.Application.Interfaces;
using AlpineGearHub.Chat.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AlpineGearHub.Chat.Infrastructure.Services;

internal sealed class SignalRChatNotifier(IHubContext<ChatHub> hubContext) : IChatNotifier
{
    public Task NotifyMessageSentAsync(Guid recipientUserId, MessageResponse message, CancellationToken ct = default) =>
        hubContext.Clients.Group(recipientUserId.ToString()).SendAsync("MessageReceived", message, ct);
}
