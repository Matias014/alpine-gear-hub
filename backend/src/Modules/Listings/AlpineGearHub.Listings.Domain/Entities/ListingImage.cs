using AlpineGearHub.SharedKernel;

namespace AlpineGearHub.Listings.Domain.Entities;

public class ListingImage : Entity
{
    public Guid ListingId { get; private set; }
    public string StorageKey { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }
    public bool IsPrimary { get; private set; }

    private ListingImage() { }

    public static ListingImage Create(Guid listingId, string storageKey, int sortOrder, bool isPrimary) =>
        new()
        {
            Id = Guid.NewGuid(),
            ListingId = listingId,
            StorageKey = storageKey,
            SortOrder = sortOrder,
            IsPrimary = isPrimary,
        };

    public void SetAsPrimary() => IsPrimary = true;
    public void UnsetAsPrimary() => IsPrimary = false;
}
