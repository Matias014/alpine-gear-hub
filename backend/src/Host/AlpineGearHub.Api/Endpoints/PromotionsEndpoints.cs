using System.Security.Claims;
using AlpineGearHub.Api.Infrastructure;
using AlpineGearHub.Listings.Application.Commands.SetListingPromoted;
using AlpineGearHub.Listings.Application.Queries.GetListingById;
using AlpineGearHub.Listings.Infrastructure.Data;
using AlpineGearHub.Promotions.Application.Commands.CreatePromotion;
using AlpineGearHub.Promotions.Application.Commands.ProcessPaymentWebhook;
using AlpineGearHub.Promotions.Application.Queries.GetPromotionById;
using AlpineGearHub.Promotions.Application.Queries.GetPromotionsByListing;
using AlpineGearHub.Promotions.Domain.Enums;
using AlpineGearHub.Promotions.Infrastructure.Data;
using MediatR;

namespace AlpineGearHub.Api.Endpoints;

public static class PromotionsEndpoints
{
    public static RouteGroupBuilder MapPromotionsEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (
            ISender sender,
            CreatePromotionRequest body,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            // Promotions never references Listings directly, so ownership is checked here in
            // the Host by querying the Listings module first (same pattern as Chat/Moderation).
            var listing = await sender.Send(new GetListingByIdQuery(body.ListingId), ct);
            if (listing is null) return Results.NotFound();

            var requesterId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (listing.SellerId != requesterId)
                throw new UnauthorizedAccessException("Only the seller can promote this listing.");

            var result = await sender.Send(new CreatePromotionCommand(body.ListingId, body.Tier), ct);

            // No real Stripe key configured - CreatePromotionCommand already settled the payment
            // synchronously (see StripePaymentGateway), so there's no webhook coming later to
            // flip IsPromoted. Do it right here instead. (Not wrapped in the cross-module
            // transaction the webhook path uses: this is a dev-only convenience fallback, not a
            // real-money path, and the listing was already confirmed to exist moments ago above.)
            if (result.PaymentStatus == "Completed")
                await sender.Send(new SetListingPromotedCommand(result.ListingId, true), ct);

            return Results.Created($"/api/promotions/{result.Id}", result);
        })
        .RequireAuthorization()
        .WithSummary("Purchase a promotion for a listing (returns a Stripe client secret)");

        group.MapGet("/{id:guid}", async (ISender sender, Guid id, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var promotion = await sender.Send(new GetPromotionByIdQuery(id), ct);
            if (promotion is null) return Results.NotFound();

            var listing = await sender.Send(new GetListingByIdQuery(promotion.ListingId), ct);
            var requesterId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isPrivileged = user.IsInRole("Moderator") || user.IsInRole("Admin");
            if (listing is not null && listing.SellerId != requesterId && !isPrivileged)
                throw new UnauthorizedAccessException("Only the listing's seller can view this promotion.");

            return Results.Ok(promotion);
        })
        .RequireAuthorization()
        .WithSummary("Get a promotion by id");

        group.MapGet("/listing/{listingId:guid}", async (
            ISender sender,
            Guid listingId,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var listing = await sender.Send(new GetListingByIdQuery(listingId), ct);
            if (listing is null) return Results.NotFound();

            var requesterId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isPrivileged = user.IsInRole("Moderator") || user.IsInRole("Admin");
            if (listing.SellerId != requesterId && !isPrivileged)
                throw new UnauthorizedAccessException("Only the listing's seller can view its promotion history.");

            var result = await sender.Send(new GetPromotionsByListingQuery(listingId), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithSummary("Get promotion history for a listing");

        group.MapPost("/webhook/stripe", async (
            HttpRequest request,
            ISender sender,
            PromotionsDbContext promotionsDb,
            ListingsDbContext listingsDb,
            CancellationToken ct) =>
        {
            using var reader = new StreamReader(request.Body);
            var payload = await reader.ReadToEndAsync(ct);
            var signature = request.Headers["Stripe-Signature"].ToString();

            // Confirming the payment and flipping IsPromoted share one transaction so a crash in
            // between can't leave a promotion marked "paid" while the listing never gets boosted.
            PaymentWebhookResult? result = null;
            await CrossModuleTransaction.RunAsync(ct, async () =>
            {
                result = await sender.Send(new ProcessPaymentWebhookCommand(payload, signature), ct);

                if (result.ListingShouldBePromoted && result.ListingId is { } listingId)
                    await sender.Send(new SetListingPromotedCommand(listingId, true), ct);
            }, promotionsDb, listingsDb);

            return Results.Ok();
        })
        .AllowAnonymous()
        .WithSummary("Stripe webhook — payment intent succeeded or failed");

        return group;
    }
}

public record CreatePromotionRequest(Guid ListingId, PromotionTier Tier);
