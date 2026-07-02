using AlpineGearHub.Promotions.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace AlpineGearHub.Promotions.Infrastructure.Services;

internal sealed class StripePaymentGateway : IPaymentGateway
{
    private readonly string _webhookSecret;

    public StripePaymentGateway(IConfiguration configuration)
    {
        StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
        _webhookSecret = configuration["Stripe:WebhookSecret"] ?? string.Empty;
    }

    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(
        decimal amount,
        string currency,
        IReadOnlyDictionary<string, string> metadata,
        CancellationToken ct = default)
    {
        var service = new PaymentIntentService();
        var intent = await service.CreateAsync(new PaymentIntentCreateOptions
        {
            // Stripe amounts are integers in the currency's smallest unit (e.g. cents).
            Amount = (long)(amount * 100),
            Currency = currency.ToLowerInvariant(),
            Metadata = metadata.ToDictionary(kv => kv.Key, kv => kv.Value),
        }, cancellationToken: ct);

        return new PaymentIntentResult(intent.Id, intent.ClientSecret);
    }

    public PaymentWebhookEvent ParseWebhookEvent(string payload, string signatureHeader)
    {
        // The Stripe account's configured API version won't always match the version this SDK
        // build was pinned against; we only read Type and the payment intent id, both stable
        // across versions, so a mismatch here shouldn't hard-fail signature-verified events.
        var stripeEvent = EventUtility.ConstructEvent(payload, signatureHeader, _webhookSecret, throwOnApiVersionMismatch: false);
        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
        return new PaymentWebhookEvent(stripeEvent.Type, paymentIntent?.Id ?? string.Empty);
    }
}
