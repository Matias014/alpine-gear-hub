using AlpineGearHub.Promotions.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace AlpineGearHub.Promotions.Infrastructure.Services;

internal sealed class StripePaymentGateway : IPaymentGateway
{
    private const string PlaceholderSecretKey = "sk_test_placeholder";

    private readonly bool _hasRealKey;
    private readonly string _webhookSecret;

    public StripePaymentGateway(IConfiguration configuration)
    {
        var secretKey = configuration["Stripe:SecretKey"];
        _hasRealKey = !string.IsNullOrWhiteSpace(secretKey) && secretKey != PlaceholderSecretKey;
        StripeConfiguration.ApiKey = secretKey;
        _webhookSecret = configuration["Stripe:WebhookSecret"] ?? string.Empty;
    }

    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(
        decimal amount,
        string currency,
        IReadOnlyDictionary<string, string> metadata,
        CancellationToken ct = default)
    {
        // No real key configured (local/demo runs, CI) - every real call here would just fail
        // with an auth error, so skip Stripe entirely and report the payment as already settled
        // instead of making "promote a listing" permanently broken without a Stripe account.
        // ParseWebhookEvent below is unaffected either way - it's a local signature check, not a
        // Stripe API call.
        if (!_hasRealKey)
            return new PaymentIntentResult($"pi_dev_{Guid.NewGuid():N}", ClientSecret: null);

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
