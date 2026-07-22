using AlpineGearHub.Promotions.Domain.Enums;
using AlpineGearHub.Promotions.Domain.Exceptions;
using AlpineGearHub.SharedKernel;

namespace AlpineGearHub.Promotions.Domain.Entities;

public class Promotion : AggregateRoot
{
    public Guid ListingId { get; private set; }
    public PromotionTier Tier { get; private set; }
    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }
    public Money Price { get; private set; } = null!;
    public PaymentStatus PaymentStatus { get; private set; }
    public string? StripePaymentIntentId { get; private set; }

    private Promotion() { }

    public static Promotion Create(Guid listingId, PromotionTier tier, Money price, int durationDays)
    {
        var now = DateTime.UtcNow;
        return new Promotion
        {
            Id = Guid.NewGuid(),
            ListingId = listingId,
            Tier = tier,
            Price = price,
            StartAt = now,
            EndAt = now.AddDays(durationDays),
            PaymentStatus = PaymentStatus.Pending,
        };
    }

    public void AttachPaymentIntent(string stripePaymentIntentId) =>
        StripePaymentIntentId = stripePaymentIntentId;

    public void MarkPaymentCompleted()
    {
        EnsurePending();
        PaymentStatus = PaymentStatus.Completed;
    }

    public void MarkPaymentFailed()
    {
        EnsurePending();
        PaymentStatus = PaymentStatus.Failed;
    }

    public void Refund()
    {
        if (PaymentStatus != PaymentStatus.Completed)
            throw new PromotionException("Only a completed promotion can be refunded.");

        PaymentStatus = PaymentStatus.Refunded;
    }

    private void EnsurePending()
    {
        if (PaymentStatus != PaymentStatus.Pending)
            throw new PromotionException("This promotion's payment has already been processed.");
    }
}
