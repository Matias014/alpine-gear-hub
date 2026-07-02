using AlpineGearHub.Chat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlpineGearHub.Chat.Infrastructure.Data.Configurations;

internal sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("conversations");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.ListingId).HasColumnName("listing_id");
        builder.Property(c => c.BuyerId).HasColumnName("buyer_id");
        builder.Property(c => c.SellerId).HasColumnName("seller_id");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
        builder.Property(c => c.LastMessageAt).HasColumnName("last_message_at").IsRequired(false);

        builder.HasMany(c => c.Messages)
            .WithOne()
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Conversation.Messages))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(c => c.BuyerId).HasDatabaseName("ix_conversations_buyer_id");
        builder.HasIndex(c => c.SellerId).HasDatabaseName("ix_conversations_seller_id");

        // Enforces "one conversation per (listing, buyer) pair" at the database level too.
        builder.HasIndex(c => new { c.ListingId, c.BuyerId })
            .IsUnique()
            .HasDatabaseName("ux_conversations_listing_buyer");
    }
}
