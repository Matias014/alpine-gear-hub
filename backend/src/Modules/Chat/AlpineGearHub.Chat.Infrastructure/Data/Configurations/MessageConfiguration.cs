using AlpineGearHub.Chat.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlpineGearHub.Chat.Infrastructure.Data.Configurations;

internal sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.ConversationId).HasColumnName("conversation_id");
        builder.Property(m => m.SenderId).HasColumnName("sender_id");
        builder.Property(m => m.Body).HasColumnName("body").HasMaxLength(2000).IsRequired();
        builder.Property(m => m.SentAt).HasColumnName("sent_at");
        builder.Property(m => m.ReadAt).HasColumnName("read_at").IsRequired(false);

        builder.HasIndex(m => m.ConversationId).HasDatabaseName("ix_messages_conversation_id");
    }
}
