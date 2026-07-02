using AlpineGearHub.Chat.Domain.Events;
using AlpineGearHub.Chat.Domain.Exceptions;
using AlpineGearHub.SharedKernel;

namespace AlpineGearHub.Chat.Domain.Entities;

public class Conversation : AggregateRoot
{
    private readonly List<Message> _messages = [];

    public Guid ListingId { get; private set; }
    public Guid BuyerId { get; private set; }
    public Guid SellerId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastMessageAt { get; private set; }

    public IReadOnlyList<Message> Messages => _messages.AsReadOnly();

    private Conversation() { }

    public static Conversation Create(Guid listingId, Guid buyerId, Guid sellerId)
    {
        if (buyerId == sellerId)
            throw new ChatException("A seller cannot start a conversation about their own listing.");

        return new Conversation
        {
            Id = Guid.NewGuid(),
            ListingId = listingId,
            BuyerId = buyerId,
            SellerId = sellerId,
            CreatedAt = DateTime.UtcNow,
        };
    }

    public bool HasParticipant(Guid userId) => userId == BuyerId || userId == SellerId;

    public Message AddMessage(Guid senderId, string body)
    {
        var message = Message.Create(Id, senderId, body);
        _messages.Add(message);
        LastMessageAt = message.SentAt;

        var recipientId = senderId == BuyerId ? SellerId : BuyerId;
        RaiseDomainEvent(new MessageSentEvent(Id, message.Id, senderId, recipientId, body, message.SentAt));

        return message;
    }

    public void MarkMessagesAsRead(Guid readerId)
    {
        foreach (var message in _messages.Where(m => m.SenderId != readerId && m.ReadAt is null))
            message.MarkAsRead();
    }
}
