using System.Net;
using System.Net.Http.Json;
using System.Text;
using AlpineGearHub.Api.Endpoints;
using AlpineGearHub.Api.Tests.Helpers;
using AlpineGearHub.Listings.Application.DTOs;
using AlpineGearHub.Promotions.Application.DTOs;
using AlpineGearHub.Promotions.Domain.Entities;
using AlpineGearHub.Promotions.Domain.Enums;
using AlpineGearHub.Promotions.Domain.ValueObjects;
using AlpineGearHub.Promotions.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AlpineGearHub.Api.Tests;

// No real Stripe test key is configured here (see appsettings.Development.json's
// sk_test_placeholder), so StripePaymentGateway's no-real-key fallback kicks in: it settles the
// payment synchronously instead of calling Stripe, which means the CreatePromotion happy path is
// actually exercisable now (see CreatePromotion_WithNoStripeKeyConfigured_...below) - it just
// exercises the dev fallback rather than a real Stripe round trip. The webhook side is exercised
// independently by seeding a Promotion straight into the DB and self-signing the payload, exactly
// like the manual verification during the Promotions phase.
[Collection(DatabaseCollection.Name)]
public sealed class PromotionsTests(AlpineGearHubApiFactory factory)
{
    [Fact]
    public async Task CreatePromotion_ByNonOwner_ReturnsForbidden()
    {
        var seller = await TestFlows.RegisterAsync(factory);
        var listing = await TestFlows.CreateAndPublishListingAsync(seller);
        var stranger = await TestFlows.RegisterAsync(factory);

        var response = await stranger.PostAsync("/api/promotions", new CreatePromotionRequest(listing.Id, PromotionTier.Standard));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreatePromotion_WithNoStripeKeyConfigured_SettlesImmediatelyAndPromotesTheListing()
    {
        var seller = await TestFlows.RegisterAsync(factory);
        var listing = await TestFlows.CreateAndPublishListingAsync(seller);

        var response = await seller.PostAsync("/api/promotions", new CreatePromotionRequest(listing.Id, PromotionTier.Standard));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var promotion = await response.Content.ReadFromJsonAsync<PromotionResponse>();
        promotion!.PaymentStatus.Should().Be("Completed");
        promotion.ClientSecret.Should().BeNull();

        var listingResponse = await seller.GetAsync($"/api/listings/{listing.Id}");
        var fetchedListing = await listingResponse.Content.ReadFromJsonAsync<ListingResponse>();
        fetchedListing!.IsPromoted.Should().BeTrue();
    }

    [Fact]
    public async Task CreatePromotion_WhenListingAlreadyHasAnActivePromotion_ReturnsUnprocessableEntity()
    {
        var seller = await TestFlows.RegisterAsync(factory);
        var listing = await TestFlows.CreateAndPublishListingAsync(seller);
        await SeedCompletedPromotionAsync(listing.Id, $"pi_seed_{Guid.NewGuid():N}");

        var response = await seller.PostAsync("/api/promotions", new CreatePromotionRequest(listing.Id, PromotionTier.Featured));

        // Never reaches Stripe - HasActivePromotionAsync rejects it first.
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Webhook_PaymentSucceeded_PromotesTheListing()
    {
        var seller = await TestFlows.RegisterAsync(factory);
        var listing = await TestFlows.CreateAndPublishListingAsync(seller);
        var paymentIntentId = $"pi_test_{Guid.NewGuid():N}";
        await SeedPendingPromotionAsync(listing.Id, paymentIntentId);

        var (payload, signature) = WebhookSigner.SignPaymentIntentEvent(paymentIntentId, "payment_intent.succeeded");
        var response = await PostWebhookAsync(payload, signature);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var listingResponse = await seller.GetAsync($"/api/listings/{listing.Id}");
        var fetched = await listingResponse.Content.ReadFromJsonAsync<ListingResponse>();
        fetched!.IsPromoted.Should().BeTrue();
    }

    [Fact]
    public async Task Webhook_PaymentFailed_LeavesListingUnpromoted()
    {
        var seller = await TestFlows.RegisterAsync(factory);
        var listing = await TestFlows.CreateAndPublishListingAsync(seller);
        var paymentIntentId = $"pi_test_{Guid.NewGuid():N}";
        await SeedPendingPromotionAsync(listing.Id, paymentIntentId);

        var (payload, signature) = WebhookSigner.SignPaymentIntentEvent(paymentIntentId, "payment_intent.payment_failed");
        var response = await PostWebhookAsync(payload, signature);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var listingResponse = await seller.GetAsync($"/api/listings/{listing.Id}");
        var fetched = await listingResponse.Content.ReadFromJsonAsync<ListingResponse>();
        fetched!.IsPromoted.Should().BeFalse();
    }

    [Fact]
    public async Task Webhook_PaymentSucceeded_ButListingNoLongerExists_RollsBackThePaymentConfirmationToo()
    {
        // Confirming the payment and flipping IsPromoted share one transaction (see
        // CrossModuleTransaction) - this proves that pairing is atomic. A promotion pointing at a
        // listing that doesn't exist (deleted, or just a bad reference) makes the second write
        // throw, and the promotion's own "payment completed" write must be rolled back with it
        // rather than leaving a promotion stuck marked "paid" for a boost that never landed.
        var buyer = await TestFlows.RegisterAsync(factory);
        var fakeListingId = Guid.NewGuid();
        var paymentIntentId = $"pi_test_{Guid.NewGuid():N}";
        var promotionId = await SeedPendingPromotionAsync(fakeListingId, paymentIntentId);

        var (payload, signature) = WebhookSigner.SignPaymentIntentEvent(paymentIntentId, "payment_intent.succeeded");
        var response = await PostWebhookAsync(payload, signature);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var promotionResponse = await buyer.GetAsync($"/api/promotions/{promotionId}");
        var fetched = await promotionResponse.Content.ReadFromJsonAsync<PromotionResponse>();
        fetched!.PaymentStatus.Should().Be("Pending");
    }

    [Fact]
    public async Task Webhook_InvalidSignature_IsRejected()
    {
        var response = await PostWebhookAsync(
            """{"type":"payment_intent.succeeded","data":{"object":{"id":"pi_fake"}}}""",
            "t=1,v1=deadbeef");

        response.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public async Task GetPromotionsForListing_ByNonOwner_ReturnsForbidden()
    {
        var seller = await TestFlows.RegisterAsync(factory);
        var listing = await TestFlows.CreateAndPublishListingAsync(seller);
        var stranger = await TestFlows.RegisterAsync(factory);

        var response = await stranger.GetAsync($"/api/promotions/listing/{listing.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task<HttpResponseMessage> PostWebhookAsync(string payload, string signatureHeader)
    {
        var client = factory.CreateClient();
        var content = new StringContent(payload, Encoding.UTF8, "application/json");
        client.DefaultRequestHeaders.Add("Stripe-Signature", signatureHeader);
        return await client.PostAsync("/api/promotions/webhook/stripe", content);
    }

    private async Task<Guid> SeedPendingPromotionAsync(Guid listingId, string paymentIntentId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PromotionsDbContext>();

        var promotion = Promotion.Create(listingId, PromotionTier.Standard, Money.Of(5m, "EUR"), 7);
        promotion.AttachPaymentIntent(paymentIntentId);

        db.Promotions.Add(promotion);
        await db.SaveChangesAsync();
        return promotion.Id;
    }

    private async Task SeedCompletedPromotionAsync(Guid listingId, string paymentIntentId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PromotionsDbContext>();

        var promotion = Promotion.Create(listingId, PromotionTier.Standard, Money.Of(5m, "EUR"), 7);
        promotion.AttachPaymentIntent(paymentIntentId);
        promotion.MarkPaymentCompleted();

        db.Promotions.Add(promotion);
        await db.SaveChangesAsync();
    }
}
