namespace AlpineGearHub.Promotions.Application.Interfaces;

// ClientSecret is null when the gateway settled the payment synchronously instead of handing
// back something for the client to confirm (see StripePaymentGateway's no-real-key fallback).
public record PaymentIntentResult(string PaymentIntentId, string? ClientSecret);

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
