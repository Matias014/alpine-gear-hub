using System.Net;
using System.Net.Http.Json;
using AlpineGearHub.Api.Endpoints;
using AlpineGearHub.Api.Tests.Helpers;
using AlpineGearHub.Chat.Application.DTOs;
using FluentAssertions;

namespace AlpineGearHub.Api.Tests;

[Collection(DatabaseCollection.Name)]
public sealed class ChatTests(AlpineGearHubApiFactory factory)
{
    [Fact]
    public async Task StartConversation_ReturnsCreated()
    {
        var seller = await TestFlows.RegisterAsync(factory);
        var listing = await TestFlows.CreateAndPublishListingAsync(seller);
        var buyer = await TestFlows.RegisterAsync(factory);

        var response = await buyer.PostAsync("/api/chat/conversations", new StartConversationRequest(listing.Id));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var conversation = await response.Content.ReadFromJsonAsync<ConversationResponse>();
        conversation!.ListingId.Should().Be(listing.Id);
    }

    [Fact]
    public async Task StartConversation_CalledTwiceBySameBuyer_ReturnsSameConversation()
    {
        var seller = await TestFlows.RegisterAsync(factory);
        var listing = await TestFlows.CreateAndPublishListingAsync(seller);
        var buyer = await TestFlows.RegisterAsync(factory);

        var first = await buyer.PostAsync("/api/chat/conversations", new StartConversationRequest(listing.Id));
        var second = await buyer.PostAsync("/api/chat/conversations", new StartConversationRequest(listing.Id));

        var firstConversation = await first.Content.ReadFromJsonAsync<ConversationResponse>();
        var secondConversation = await second.Content.ReadFromJsonAsync<ConversationResponse>();
        secondConversation!.Id.Should().Be(firstConversation!.Id);
    }

    [Fact]
    public async Task StartConversation_SellerMessagingOwnListing_ReturnsUnprocessableEntity()
    {
        var seller = await TestFlows.RegisterAsync(factory);
        var listing = await TestFlows.CreateAndPublishListingAsync(seller);

        var response = await seller.PostAsync("/api/chat/conversations", new StartConversationRequest(listing.Id));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task SendMessage_BothSidesSeeItInHistory()
    {
        var seller = await TestFlows.RegisterAsync(factory);
        var listing = await TestFlows.CreateAndPublishListingAsync(seller);
        var buyer = await TestFlows.RegisterAsync(factory);
        var startResponse = await buyer.PostAsync("/api/chat/conversations", new StartConversationRequest(listing.Id));
        var conversation = (await startResponse.Content.ReadFromJsonAsync<ConversationResponse>())!;

        await buyer.PostAsync($"/api/chat/conversations/{conversation.Id}/messages", new SendMessageRequest("Is this still available?"));
        await seller.PostAsync($"/api/chat/conversations/{conversation.Id}/messages", new SendMessageRequest("Yes!"));

        var historyResponse = await buyer.GetAsync($"/api/chat/conversations/{conversation.Id}/messages");
        var messages = await historyResponse.Content.ReadFromJsonAsync<List<MessageResponse>>();

        messages.Should().HaveCount(2);
        messages.Should().Contain(m => m.Body == "Is this still available?");
        messages.Should().Contain(m => m.Body == "Yes!");
    }

    [Fact]
    public async Task GetMessages_ByNonParticipant_ReturnsForbidden()
    {
        var seller = await TestFlows.RegisterAsync(factory);
        var listing = await TestFlows.CreateAndPublishListingAsync(seller);
        var buyer = await TestFlows.RegisterAsync(factory);
        var startResponse = await buyer.PostAsync("/api/chat/conversations", new StartConversationRequest(listing.Id));
        var conversation = (await startResponse.Content.ReadFromJsonAsync<ConversationResponse>())!;

        var stranger = await TestFlows.RegisterAsync(factory);
        var response = await stranger.GetAsync($"/api/chat/conversations/{conversation.Id}/messages");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task MarkAsRead_ClearsUnreadCountForTheReader()
    {
        var seller = await TestFlows.RegisterAsync(factory);
        var listing = await TestFlows.CreateAndPublishListingAsync(seller);
        var buyer = await TestFlows.RegisterAsync(factory);
        var startResponse = await buyer.PostAsync("/api/chat/conversations", new StartConversationRequest(listing.Id));
        var conversation = (await startResponse.Content.ReadFromJsonAsync<ConversationResponse>())!;
        await buyer.PostAsync($"/api/chat/conversations/{conversation.Id}/messages", new SendMessageRequest("Hello"));

        var beforeResponse = await seller.GetAsync("/api/chat/conversations");
        var before = await beforeResponse.Content.ReadFromJsonAsync<List<ConversationSummaryResponse>>();
        before.Should().Contain(c => c.Id == conversation.Id && c.UnreadCount == 1);

        var readResponse = await seller.PostAsync($"/api/chat/conversations/{conversation.Id}/read");
        readResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var afterResponse = await seller.GetAsync("/api/chat/conversations");
        var after = await afterResponse.Content.ReadFromJsonAsync<List<ConversationSummaryResponse>>();
        after.Should().Contain(c => c.Id == conversation.Id && c.UnreadCount == 0);
    }
}
