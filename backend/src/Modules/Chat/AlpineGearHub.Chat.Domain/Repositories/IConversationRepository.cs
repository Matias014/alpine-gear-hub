using AlpineGearHub.Chat.Domain.Entities;

namespace AlpineGearHub.Chat.Domain.Repositories;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Conversation?> GetByListingAndBuyerAsync(Guid listingId, Guid buyerId, CancellationToken ct = default);
    Task<IReadOnlyList<Conversation>> GetForUserAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Conversation conversation, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
