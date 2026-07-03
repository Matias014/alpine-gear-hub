using System.Security.Cryptography;
using System.Text;

namespace AlpineGearHub.Api.Tests.Helpers;

// Signs a Stripe-shaped webhook payload myself using the shared secret, so I can test the
// webhook endpoint without a real Stripe account - same trick I used manually with curl earlier.
public static class WebhookSigner
{
    public const string Secret = "whsec_test_secret_for_integration_tests";

    public static (string Payload, string SignatureHeader) SignPaymentIntentEvent(string paymentIntentId, string eventType)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var payload = $$"""
            {
              "id": "evt_test_{{paymentIntentId}}",
              "object": "event",
              "api_version": "2024-06-20",
              "created": {{timestamp}},
              "type": "{{eventType}}",
              "livemode": false,
              "pending_webhooks": 1,
              "request": { "id": null, "idempotency_key": null },
              "data": {
                "object": {
                  "id": "{{paymentIntentId}}",
                  "object": "payment_intent",
                  "amount": 500,
                  "currency": "eur",
                  "status": "succeeded",
                  "livemode": false,
                  "metadata": {}
                },
                "previous_attributes": {}
              }
            }
            """;

        var signedPayload = $"{timestamp}.{payload}";
        var signatureBytes = HMACSHA256.HashData(Encoding.UTF8.GetBytes(Secret), Encoding.UTF8.GetBytes(signedPayload));
        var signature = Convert.ToHexStringLower(signatureBytes);

        return (payload, $"t={timestamp},v1={signature}");
    }
}
