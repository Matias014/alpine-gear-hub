using AlpineGearHub.Listings.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlpineGearHub.Listings.Infrastructure.Data.Configurations;

internal sealed class ListingImageConfiguration : IEntityTypeConfiguration<ListingImage>
{
    public void Configure(EntityTypeBuilder<ListingImage> builder)
    {
        builder.ToTable("listing_images");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).HasColumnName("id");
        builder.Property(i => i.ListingId).HasColumnName("listing_id");
        builder.Property(i => i.StorageKey).HasColumnName("storage_key").HasMaxLength(512).IsRequired();
        builder.Property(i => i.SortOrder).HasColumnName("sort_order");
        builder.Property(i => i.IsPrimary).HasColumnName("is_primary");
    }
}
