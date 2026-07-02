namespace AlpineGearHub.Promotions.Application.Interfaces;

public record PaymentIntentResult(string PaymentIntentId, string ClientSecret);

public record PaymentWebhookEvent(string Type, string PaymentIntentId);

public interface IPaymentGateway
{
    Task<PaymentIntentResult> CreatePaymentIntentAsync(
        decimal amount,
        string currency,
        IReadOnlyDictionary<string, string> metadata,
        CancellationToken ct = default);

    PaymentWebhookEvent ParseWebhookEvent(string payload, string signatureHeader);
}
