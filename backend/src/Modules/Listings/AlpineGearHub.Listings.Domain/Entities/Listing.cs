using AlpineGearHub.Listings.Domain.Enums;
using AlpineGearHub.Listings.Domain.Events;
using AlpineGearHub.Listings.Domain.Exceptions;
using AlpineGearHub.Listings.Domain.ValueObjects;
using AlpineGearHub.SharedKernel;

namespace AlpineGearHub.Listings.Domain.Entities;

public class Listing : AggregateRoot
{
    private const int MaxImages = 8;
    private const int ListingLifetimeDays = 60;

    private readonly List<ListingImage> _images = [];

    public Guid SellerId { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Money Price { get; private set; } = null!;
    public GearCondition Condition { get; private set; }
    public ListingStatus Status { get; private set; }
    public string Location { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    public IReadOnlyList<ListingImage> Images => _images.AsReadOnly();

    private Listing() { }

    public static Listing Create(
        Guid sellerId,
        Guid categoryId,
        string title,
        string description,
        Money price,
        GearCondition condition,
        string location)
    {
        var now = DateTime.UtcNow;
        return new Listing
        {
            Id = Guid.NewGuid(),
            SellerId = sellerId,
            CategoryId = categoryId,
            Title = title,
            Description = description,
            Price = price,
            Condition = condition,
            Location = location,
            Status = ListingStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    public void Update(string title, string description, Money price, GearCondition condition, string location)
    {
        if (Status != ListingStatus.Draft && Status != ListingStatus.Active)
            throw new InvalidListingStatusTransitionException(Status, Status);

        Title = title;
        Description = description;
        Price = price;
        Condition = condition;
        Location = location;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Publish()
    {
        if (Status != ListingStatus.Draft)
            throw new InvalidListingStatusTransitionException(Status, ListingStatus.Active);

        Status = ListingStatus.Active;
        ExpiresAt = DateTime.UtcNow.AddDays(ListingLifetimeDays);
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new ListingPublishedEvent(Id, SellerId));
    }

    public void MarkAsReserved()
    {
        if (Status != ListingStatus.Active)
            throw new InvalidListingStatusTransitionException(Status, ListingStatus.Reserved);

        Status = ListingStatus.Reserved;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsSold()
    {
        if (Status != ListingStatus.Active && Status != ListingStatus.Reserved)
            throw new InvalidListingStatusTransitionException(Status, ListingStatus.Sold);

        Status = ListingStatus.Sold;
        ExpiresAt = null;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new ListingSoldEvent(Id, SellerId));
    }

    public void Renew()
    {
        if (Status != ListingStatus.Expired)
            throw new InvalidListingStatusTransitionException(Status, ListingStatus.Active);

        Status = ListingStatus.Active;
        ExpiresAt = DateTime.UtcNow.AddDays(ListingLifetimeDays);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Remove()
    {
        if (Status == ListingStatus.Sold || Status == ListingStatus.Removed)
            throw new InvalidListingStatusTransitionException(Status, ListingStatus.Removed);

        Status = ListingStatus.Removed;
        ExpiresAt = null;
        UpdatedAt = DateTime.UtcNow;
        RaiseDomainEvent(new ListingRemovedEvent(Id, SellerId));
    }

    public ListingImage AddImage(string storageKey)
    {
        if (_images.Count >= MaxImages)
            throw new ListingException($"A listing cannot have more than {MaxImages} images.");

        var isPrimary = _images.Count == 0;
        var sortOrder = _images.Count;
        var image = ListingImage.Create(Id, storageKey, sortOrder, isPrimary);
        _images.Add(image);
        UpdatedAt = DateTime.UtcNow;
        return image;
    }

    public void RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId)
            ?? throw new ListingException($"Image '{imageId}' not found on this listing.");

        _images.Remove(image);

        if (image.IsPrimary && _images.Count > 0)
            _images[0].SetAsPrimary();

        UpdatedAt = DateTime.UtcNow;
    }
}
