using System.Security.Claims;
using AlpineGearHub.Chat.Application.Commands.MarkConversationAsRead;
using AlpineGearHub.Chat.Application.Commands.SendMessage;
using AlpineGearHub.Chat.Application.Commands.StartConversation;
using AlpineGearHub.Chat.Application.Queries.GetConversationMessages;
using AlpineGearHub.Chat.Application.Queries.GetConversations;
using AlpineGearHub.Listings.Application.Queries.GetListingById;
using MediatR;

namespace AlpineGearHub.Api.Endpoints;

public static class ChatEndpoints
{
    public static RouteGroupBuilder MapChatEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet("/conversations", async (ISender sender, ClaimsPrincipal user, CancellationToken ct) =>
        {
            var userId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await sender.Send(new GetConversationsQuery(userId), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithSummary("Get conversations for the current user");

        group.MapPost("/conversations", async (
            ISender sender,
            StartConversationRequest body,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var buyerId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // The Chat module never references Listings directly (modules stay decoupled), so the
            // seller id is resolved here in the Host by querying the Listings module first.
            var listing = await sender.Send(new GetListingByIdQuery(body.ListingId), ct);
            if (listing is null) return Results.NotFound();

            var result = await sender.Send(new StartConversationCommand(body.ListingId, buyerId, listing.SellerId), ct);
            return Results.Created($"/api/chat/conversations/{result.Id}", result);
        })
        .RequireAuthorization()
        .WithSummary("Start (or resume) a conversation about a listing");

        group.MapGet("/conversations/{id:guid}/messages", async (
            ISender sender,
            Guid id,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var requesterId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await sender.Send(new GetConversationMessagesQuery(id, requesterId), ct);
            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithSummary("Get messages in a conversation");

        group.MapPost("/conversations/{id:guid}/messages", async (
            ISender sender,
            Guid id,
            SendMessageRequest body,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var senderId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await sender.Send(new SendMessageCommand(id, senderId, body.Body), ct);
            return Results.Created($"/api/chat/conversations/{id}/messages/{result.Id}", result);
        })
        .RequireAuthorization()
        .WithSummary("Send a message in a conversation");

        group.MapPost("/conversations/{id:guid}/read", async (
            ISender sender,
            Guid id,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var requesterId = Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await sender.Send(new MarkConversationAsReadCommand(id, requesterId), ct);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .WithSummary("Mark all messages in a conversation as read");

        return group;
    }
}

public record StartConversationRequest(Guid ListingId);
public record SendMessageRequest(string Body);
