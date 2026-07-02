using AlpineGearHub.Promotions.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlpineGearHub.Promotions.Infrastructure.Data.Configurations;

internal sealed class PromotionConfiguration : IEntityTypeConfiguration<Promotion>
{
    public void Configure(EntityTypeBuilder<Promotion> builder)
    {
        builder.ToTable("promotions");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.ListingId).HasColumnName("listing_id");
        builder.Property(p => p.Tier).HasColumnName("tier").HasConversion<string>().HasMaxLength(50);
        builder.Property(p => p.StartAt).HasColumnName("start_at");
        builder.Property(p => p.EndAt).HasColumnName("end_at");
        builder.Property(p => p.PaymentStatus).HasColumnName("payment_status").HasConversion<string>().HasMaxLength(50);
        builder.Property(p => p.StripePaymentIntentId).HasColumnName("stripe_payment_intent_id").HasMaxLength(255).IsRequired(false);

        builder.OwnsOne(p => p.Price, money =>
        {
            money.Property(m => m.Amount).HasColumnName("price_amount").HasColumnType("numeric(18,2)");
            money.Property(m => m.Currency).HasColumnName("price_currency").HasMaxLength(3);
        });

        builder.HasIndex(p => p.ListingId).HasDatabaseName("ix_promotions_listing_id");
        builder.HasIndex(p => p.StripePaymentIntentId).IsUnique().HasDatabaseName("ux_promotions_stripe_payment_intent_id");
    }
}
