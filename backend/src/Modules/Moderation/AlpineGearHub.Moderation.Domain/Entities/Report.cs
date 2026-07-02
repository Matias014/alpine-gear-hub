using AlpineGearHub.Moderation.Domain.Enums;
using AlpineGearHub.Moderation.Domain.Exceptions;
using AlpineGearHub.SharedKernel;

namespace AlpineGearHub.Moderation.Domain.Entities;

public class Report : AggregateRoot
{
    public Guid ListingId { get; private set; }
    public Guid ReportedByUserId { get; private set; }
    public ReportReason Reason { get; private set; }
    public string? Description { get; private set; }
    public ReportStatus Status { get; private set; }
    public Guid? ReviewedByUserId { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Report() { }

    public static Report Create(Guid listingId, Guid reportedByUserId, ReportReason reason, string? description) =>
        new()
        {
            Id = Guid.NewGuid(),
            ListingId = listingId,
            ReportedByUserId = reportedByUserId,
            Reason = reason,
            Description = description,
            Status = ReportStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };

    public void MarkReviewed(Guid reviewerId)
    {
        EnsurePending();
        Status = ReportStatus.Reviewed;
        ReviewedByUserId = reviewerId;
        ReviewedAt = DateTime.UtcNow;
    }

    public void Dismiss(Guid reviewerId)
    {
        EnsurePending();
        Status = ReportStatus.Dismissed;
        ReviewedByUserId = reviewerId;
        ReviewedAt = DateTime.UtcNow;
    }

    private void EnsurePending()
    {
        if (Status != ReportStatus.Pending)
            throw new ModerationException("This report has already been reviewed.");
    }
}
