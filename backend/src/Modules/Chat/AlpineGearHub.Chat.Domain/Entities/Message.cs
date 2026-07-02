using AlpineGearHub.SharedKernel;

namespace AlpineGearHub.Chat.Domain.Entities;

public class Message : Entity
{
    public Guid ConversationId { get; private set; }
    public Guid SenderId { get; private set; }
    public string Body { get; private set; } = string.Empty;
    public DateTime SentAt { get; private set; }
    public DateTime? ReadAt { get; private set; }

    private Message() { }

    public static Message Create(Guid conversationId, Guid senderId, string body) =>
        new()
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderId = senderId,
            Body = body,
            SentAt = DateTime.UtcNow,
        };

    public void MarkAsRead() => ReadAt ??= DateTime.UtcNow;
}
