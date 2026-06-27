using AlpineGearHub.Listings.Domain.Entities;
using AlpineGearHub.Listings.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlpineGearHub.Listings.Infrastructure.Data.Configurations;

internal sealed class ListingConfiguration : IEntityTypeConfiguration<Listing>
{
    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        builder.ToTable("listings");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id).HasColumnName("id");
        builder.Property(l => l.SellerId).HasColumnName("seller_id");
        builder.Property(l => l.CategoryId).HasColumnName("category_id");
        builder.Property(l => l.Title).HasColumnName("title").HasMaxLength(120).IsRequired();
        builder.Property(l => l.Description).HasColumnName("description").HasMaxLength(3000).IsRequired();
        builder.Property(l => l.Condition).HasColumnName("condition").HasConversion<string>().HasMaxLength(50);
        builder.Property(l => l.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50);
        builder.Property(l => l.Location).HasColumnName("location").HasMaxLength(120).IsRequired();
        builder.Property(l => l.CreatedAt).HasColumnName("created_at");
        builder.Property(l => l.UpdatedAt).HasColumnName("updated_at");
        builder.Property(l => l.ExpiresAt).HasColumnName("expires_at").IsRequired(false);

        builder.OwnsOne(l => l.Price, money =>
        {
            money.Property(m => m.Amount).HasColumnName("price_amount").HasColumnType("numeric(18,2)");
            money.Property(m => m.Currency).HasColumnName("price_currency").HasMaxLength(3);
        });

        builder.HasMany(l => l.Images)
            .WithOne()
            .HasForeignKey(i => i.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Listing.Images))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(l => l.SellerId).HasDatabaseName("ix_listings_seller_id");
        builder.HasIndex(l => l.CategoryId).HasDatabaseName("ix_listings_category_id");
        builder.HasIndex(l => l.Status).HasDatabaseName("ix_listings_status");
    }
}
