using System.Security.Claims;
using AlpineGearHub.Listings.Application.Commands.ChangeListingStatus;
using AlpineGearHub.Listings.Application.Commands.CreateListing;
using AlpineGearHub.Listings.Application.Commands.DeleteListingImage;
using AlpineGearHub.Listings.Application.Commands.PublishListing;
using AlpineGearHub.Listings.Application.Commands.UpdateListing;
using AlpineGearHub.Listings.Application.Commands.UploadListingImage;
using AlpineGearHub.Listings.Application.Queries.GetListingById;
using AlpineGearHub.Listings.Application.Queries.GetListings;
using AlpineGearHub.Listings.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AlpineGearHub.Api.Endpoints;

public static class ListingEndpoints
{
    public static RouteGroupBuilder MapListingEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/", async (
            ISender sender,
            [FromQuery] Guid? categoryId,
            [FromQuery] string? condition,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] string? search,
            [FromQuery] Guid? sellerId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default) =>
        {
            var result = await sender.Send(
                new GetListingsQuery(categoryId, condition, minPrice, maxPrice, search, sellerId, page, pageSize), ct);
            return Results.Ok(result);
        })
        .AllowAnonymous()
        .WithSummary("Get paginated listings with optional filters");

        group.MapGet("/{id:guid}", async (ISender sender, Guid id, CancellationToken ct) =>
        {
            var result = await sender.Send(new GetListingByIdQuery(id), ct);
            return result is null ? Results.NotFound() : Results.Ok(result);
        })
        .AllowAnonymous()
        .WithSummary("Get listing by ID");

        group.MapPost("/", async (ISender sender, CreateListingCommand command, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var sellerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await sender.Send(command with { SellerId = sellerId }, ct);
            return Results.Created($"/api/listings/{result.Id}", result);
        })
        .RequireAuthorization()
        .WithSummary("Create a new listing (Draft)");

        group.MapPut("/{id:guid}", async (
            ISender sender,
            Guid id,
            UpdateListingCommand command,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var requesterId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await sender.Send(command with { ListingId = id, RequesterId = requesterId }, ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithSummary("Update a listing (only seller)");

        group.MapPost("/{id:guid}/publish", async (ISender sender, Guid id, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var requesterId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await sender.Send(new PublishListingCommand(id, requesterId), ct);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithSummary("Publish a Draft listing → Active");

        group.MapPost("/{id:guid}/status", async (
            ISender sender,
            Guid id,
            [FromBody] ChangeStatusRequest body,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var requesterId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var isPrivileged = user.IsInRole("Moderator") || user.IsInRole("Admin");
            await sender.Send(new ChangeListingStatusCommand(id, requesterId, isPrivileged, body.Action), ct);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithSummary("Change listing status (reserve / sell / renew / remove)");

        group.MapPost("/{id:guid}/images", async (
            ISender sender,
            Guid id,
            IFormFile file,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var requesterId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await using var stream = file.OpenReadStream();
            var result = await sender.Send(
                new UploadListingImageCommand(id, requesterId, stream), ct);
            return Results.Created($"/api/listings/{id}", result);
        })
        .RequireAuthorization()
        .DisableAntiforgery()
        .WithSummary("Upload an image to a listing");

        group.MapDelete("/{id:guid}/images/{imageId:guid}", async (
            ISender sender,
            Guid id,
            Guid imageId,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var requesterId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await sender.Send(new DeleteListingImageCommand(id, imageId, requesterId), ct);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithSummary("Delete an image from a listing");

        return group;
    }
}

public record ChangeStatusRequest(ListingStatusAction Action);
