using System.Net;
using System.Net.Http.Json;
using AlpineGearHub.Api.Endpoints;
using AlpineGearHub.Api.Tests.Helpers;
using AlpineGearHub.Identity.Application.Commands.Login;
using AlpineGearHub.Identity.Application.DTOs;
using AlpineGearHub.Listings.Application.Commands.ChangeListingStatus;
using AlpineGearHub.Listings.Application.Commands.CreateListing;
using AlpineGearHub.Listings.Application.Commands.UpdateListing;
using AlpineGearHub.Listings.Application.DTOs;
using AlpineGearHub.Listings.Domain.Enums;
using FluentAssertions;

namespace AlpineGearHub.Api.Tests;

[Collection(DatabaseCollection.Name)]
public sealed class ListingsTests(AlpineGearHubApiFactory factory)
{
    [Fact]
    public async Task CreateListing_ReturnsDraft()
    {
        var seller = await TestFlows.RegisterAsync(factory);
        var categoryId = await TestFlows.GetAnyCategoryIdAsync(seller);

        var response = await seller.PostAsync("/api/listings", new CreateListingCommand(
            Guid.Empty, categoryId, "Petzl GriGri", "Barely used", 80m, "EUR", GearCondition.LikeNew, "Chamonix"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var listing = await response.Content.ReadFromJsonAsync<ListingResponse>();
        listing!.Status.Should().Be("Draft");
        listing.IsPromoted.Should().BeFalse();
    }

    [Fact]
    public async Task CreateListing_WithoutAuth_ReturnsUnauthorized()
    {
        var anonymous = new ApiClient(factory.CreateClient());

        var response = await anonymous.PostAsync("/api/listings", new CreateListingCommand(
            Guid.Empty, Guid.NewGuid(), "No Auth", "desc", 10m, "EUR", GearCondition.Good, "Nowhere"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Publish_ByOwner_MakesListingActive()
    {
        var seller = await TestFlows.RegisterAsync(factory);
        var listing = await TestFlows.CreateAndPublishListingAsync(seller);

        var getResponse = await seller.GetAsync($"/api/listings/{listing.Id}");
        var fetched = await getResponse.Content.ReadFromJsonAsync<ListingResponse>();

        fetched!.Status.Should().Be("Active");
        fetched.ExpiresAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Publish_ByNonOwner_ReturnsForbidden()
    {
        var seller = await TestFlows.RegisterAsync(factory);
        var categoryId = await TestFlows.GetAnyCategoryIdAsync(seller);
        var createResponse = await seller.PostAsync("/api/listings", new CreateListingCommand(
            Guid.Empty, categoryId, "Someone Else's Listing", "desc", 20m, "EUR", GearCondition.Fair, "Alps"));
        var listing = (await createResponse.Content.ReadFromJsonAsync<ListingResponse>())!;

        var stranger = await TestFlows.RegisterAsync(factory);
        var response = await stranger.PostAsync($"/api/listings/{listing.Id}/publish");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Update_ByOwner_ChangesFields()
    {
        var seller = await TestFlows.RegisterAsync(factory);
        var listing = await TestFlows.CreateAndPublishListingAsync(seller);

        var response = await seller.PutAsync($"/api/listings/{listing.Id}", new UpdateListingCommand(
            listing.Id, Guid.Empty, "Updated Title", "Updated description", 99m, "EUR", GearCondition.Fair, "New City"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.Content.ReadFromJsonAsync<ListingResponse>();
        updated!.Title.Should().Be("Updated Title");
        updated.Price.Should().Be(99m);
    }

    [Fact]
    public async Task Update_ByNonOwner_ReturnsForbidden()
    {
        var seller = await TestFlows.RegisterAsync(factory);
        var listing = await TestFlows.CreateAndPublishListingAsync(seller);
        var stranger = await TestFlows.RegisterAsync(factory);

        var response = await stranger.PutAsync($"/api/listings/{listing.Id}", new UpdateListingCommand(
            listing.Id, Guid.Empty, "Hijacked", "desc", 1m, "EUR", GearCondition.Poor, "Nowhere"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task StatusTransitions_ReserveThenSell_Succeeds()
    {
        var seller = await TestFlows.RegisterAsync(factory);
        var listing = await TestFlows.CreateAndPublishListingAsync(seller);

        var reserveResponse = await seller.PostAsync($"/api/listings/{listing.Id}/status",
            new ChangeStatusRequest(ListingStatusAction.Reserve));
        reserveResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var sellResponse = await seller.PostAsync($"/api/listings/{listing.Id}/status",
            new ChangeStatusRequest(ListingStatusAction.Sell));
        sellResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await seller.GetAsync($"/api/listings/{listing.Id}");
        var fetched = await getResponse.Content.ReadFromJsonAsync<ListingResponse>();
        fetched!.Status.Should().Be("Sold");
    }

    [Fact]
    public async Task StatusTransition_SoldListingCannotBeReserved_ReturnsUnprocessableEntity()
    {
        var seller = await TestFlows.RegisterAsync(factory);
        var listing = await TestFlows.CreateAndPublishListingAsync(seller);
        await seller.PostAsync($"/api/listings/{listing.Id}/status", new ChangeStatusRequest(ListingStatusAction.Sell));

        var response = await seller.PostAsync($"/api/listings/{listing.Id}/status",
            new ChangeStatusRequest(ListingStatusAction.Reserve));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Remove_ByRegularStranger_ReturnsForbidden()
    {
        var seller = await TestFlows.RegisterAsync(factory);
        var listing = await TestFlows.CreateAndPublishListingAsync(seller);
        var stranger = await TestFlows.RegisterAsync(factory);

        var response = await stranger.PostAsync($"/api/listings/{listing.Id}/status",
            new ChangeStatusRequest(ListingStatusAction.Remove));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Remove_ByAdmin_Succeeds()
    {
        var seller = await TestFlows.RegisterAsync(factory);
        var listing = await TestFlows.CreateAndPublishListingAsync(seller);

        var admin = new ApiClient(factory.CreateClient());
        var loginResponse = await admin.PostAsync("/api/auth/login",
            new LoginCommand("admin@alpinegearhub.local", "Admin1234!"));
        var auth = (await loginResponse.Content.ReadFromJsonAsync<AuthResponse>())!;
        admin.SetBearerToken(auth.AccessToken);

        var response = await admin.PostAsync($"/api/listings/{listing.Id}/status",
            new ChangeStatusRequest(ListingStatusAction.Remove));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var getResponse = await seller.GetAsync($"/api/listings/{listing.Id}");
        var fetched = await getResponse.Content.ReadFromJsonAsync<ListingResponse>();
        fetched!.Status.Should().Be("Removed");
    }

    [Fact]
    public async Task GetListings_SearchFiltersToMatchingTitleOnly()
    {
        var seller = await TestFlows.RegisterAsync(factory);
        var uniqueWord = Guid.NewGuid().ToString("N")[..12];
        await TestFlows.CreateAndPublishListingAsync(seller, $"Rare {uniqueWord} Ice Axe");
        await TestFlows.CreateAndPublishListingAsync(seller, "An unrelated harness");

        var response = await seller.GetAsync($"/api/listings?search={uniqueWord}");

        var page = await response.Content.ReadFromJsonAsync<PagedResponse<ListingSummaryResponse>>();
        page!.Items.Should().ContainSingle(l => l.Title.Contains(uniqueWord));
    }

    [Fact]
    public async Task GetListingById_UnknownId_ReturnsNotFound()
    {
        var anonymous = new ApiClient(factory.CreateClient());

        var response = await anonymous.GetAsync($"/api/listings/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
