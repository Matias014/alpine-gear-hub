using MediatR;

namespace AlpineGearHub.Promotions.Application.Commands.ProcessPaymentWebhook;

public record ProcessPaymentWebhookCommand(string Payload, string SignatureHeader) : IRequest<PaymentWebhookResult>;

public record PaymentWebhookResult(Guid? PromotionId, Guid? ListingId, bool ListingShouldBePromoted);
