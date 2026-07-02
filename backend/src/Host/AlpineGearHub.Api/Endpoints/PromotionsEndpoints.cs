using System.Security.Claims;
using AlpineGearHub.Listings.Application.Commands.SetListingPromoted;
using AlpineGearHub.Listings.Application.Queries.GetListingById;
using AlpineGearHub.Promotions.Application.Commands.CreatePromotion;
using AlpineGearHub.Promotions.Application.Commands.ProcessPaymentWebhook;
using AlpineGearHub.Promotions.Application.Queries.GetPromotionById;
using AlpineGearHub.Promotions.Application.Queries.GetPromotionsByListing;
using AlpineGearHub.Promotions.Domain.Enums;
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

        group.MapPost("/webhook/stripe", async (HttpRequest request, ISender sender, CancellationToken ct) =>
        {
            using var reader = new StreamReader(request.Body);
            var payload = await reader.ReadToEndAsync(ct);
            var signature = request.Headers["Stripe-Signature"].ToString();

            var result = await sender.Send(new ProcessPaymentWebhookCommand(payload, signature), ct);

            if (result.ListingShouldBePromoted && result.ListingId is { } listingId)
                await sender.Send(new SetListingPromotedCommand(listingId, true), ct);

            return Results.Ok();
        })
        .AllowAnonymous()
        .WithSummary("Stripe webhook — payment intent succeeded or failed");

        return group;
    }
}

public record CreatePromotionRequest(Guid ListingId, PromotionTier Tier);
