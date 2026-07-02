using AlpineGearHub.Chat.Application.DTOs;

namespace AlpineGearHub.Chat.Application.Interfaces;

public interface IChatNotifier
{
    Task NotifyMessageSentAsync(Guid recipientUserId, MessageResponse message, CancellationToken ct = default);
}
