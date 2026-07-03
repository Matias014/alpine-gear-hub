using System.Net;
using System.Net.Http.Json;
using System.Text;
using AlpineGearHub.Api.Endpoints;
using AlpineGearHub.Api.Tests.Helpers;
using AlpineGearHub.Listings.Application.DTOs;
using AlpineGearHub.Promotions.Domain.Entities;
using AlpineGearHub.Promotions.Domain.Enums;
using AlpineGearHub.Promotions.Domain.ValueObjects;
using AlpineGearHub.Promotions.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AlpineGearHub.Api.Tests;

// CreatePromotionCommand calls the real Stripe API, which always fails here since there's no
// real test key configured (see appsettings.Development.json's sk_test_placeholder - same story
// as when I tested this by hand). So these tests only cover what doesn't reach that call: the
// ownership check (fires in the Host before the command even runs) and the duplicate-active-
// promotion guard (fires inside the handler before it calls Stripe). The webhook side is
// exercised independently by seeding a Promotion straight into the DB and self-signing the
// payload, exactly like the manual verification during Promotions phase.
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

    private async Task SeedPendingPromotionAsync(Guid listingId, string paymentIntentId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PromotionsDbContext>();

        var promotion = Promotion.Create(listingId, PromotionTier.Standard, Money.Of(5m, "EUR"), 7);
        promotion.AttachPaymentIntent(paymentIntentId);

        db.Promotions.Add(promotion);
        await db.SaveChangesAsync();
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
