using AlpineGearHub.Promotions.Application.Interfaces;
using AlpineGearHub.Promotions.Domain.Repositories;
using MediatR;

namespace AlpineGearHub.Promotions.Application.Commands.ProcessPaymentWebhook;

internal sealed class ProcessPaymentWebhookCommandHandler(
    IPromotionRepository promotionRepository,
    IPaymentGateway paymentGateway) : IRequestHandler<ProcessPaymentWebhookCommand, PaymentWebhookResult>
{
    public async Task<PaymentWebhookResult> Handle(ProcessPaymentWebhookCommand request, CancellationToken cancellationToken)
    {
        var webhookEvent = paymentGateway.ParseWebhookEvent(request.Payload, request.SignatureHeader);

        var promotion = await promotionRepository.GetByStripePaymentIntentIdAsync(webhookEvent.PaymentIntentId, cancellationToken);
        if (promotion is null) return new PaymentWebhookResult(null, null, false);

        switch (webhookEvent.Type)
        {
            case "payment_intent.succeeded":
                promotion.MarkPaymentCompleted();
                await promotionRepository.SaveChangesAsync(cancellationToken);
                return new PaymentWebhookResult(promotion.Id, promotion.ListingId, true);

            case "payment_intent.payment_failed":
                promotion.MarkPaymentFailed();
                await promotionRepository.SaveChangesAsync(cancellationToken);
                return new PaymentWebhookResult(promotion.Id, promotion.ListingId, false);

            default:
                return new PaymentWebhookResult(null, null, false);
        }
    }
}
