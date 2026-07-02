using AlpineGearHub.Chat.Domain.Entities;
using AlpineGearHub.Chat.Domain.Repositories;
using AlpineGearHub.Chat.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlpineGearHub.Chat.Infrastructure.Repositories;

internal sealed class ConversationRepository(ChatDbContext db) : IConversationRepository
{
    public async Task<Conversation?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await db.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<Conversation?> GetByListingAndBuyerAsync(Guid listingId, Guid buyerId, CancellationToken ct = default) =>
        await db.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.ListingId == listingId && c.BuyerId == buyerId, ct);

    public async Task<IReadOnlyList<Conversation>> GetForUserAsync(Guid userId, CancellationToken ct = default) =>
        await db.Conversations
            .Include(c => c.Messages)
            .Where(c => c.BuyerId == userId || c.SellerId == userId)
            .ToListAsync(ct);

    public async Task AddAsync(Conversation conversation, CancellationToken ct = default) =>
        await db.Conversations.AddAsync(conversation, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
