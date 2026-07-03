using System.Net.Http.Json;
using AlpineGearHub.Identity.Application.Commands.Register;
using AlpineGearHub.Identity.Application.DTOs;
using AlpineGearHub.Listings.Application.Commands.CreateListing;
using AlpineGearHub.Listings.Application.DTOs;
using AlpineGearHub.Listings.Domain.Enums;

namespace AlpineGearHub.Api.Tests.Helpers;

// Grew this out of copy-pasted register/login/create-listing boilerplate that showed up in
// nearly every test class - most module tests need at least one authenticated user to start from.
public static class TestFlows
{
    public static async Task<ApiClient> RegisterAsync(
        AlpineGearHubApiFactory factory, string fullName = "Test User")
    {
        var email = $"{Guid.NewGuid():N}@test.local";
        var client = new ApiClient(factory.CreateClient());

        var response = await client.PostAsync("/api/auth/register", new RegisterCommand(fullName, email, "Password1!"));
        response.EnsureSuccessStatusCode();

        var auth = (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
        client.SetBearerToken(auth.AccessToken);

        return client;
    }

    public static async Task<Guid> GetAnyCategoryIdAsync(ApiClient client)
    {
        var response = await client.GetAsync("/api/categories");
        response.EnsureSuccessStatusCode();
        var categories = (await response.Content.ReadFromJsonAsync<List<CategoryResponse>>())!;
        return categories[0].Id;
    }

    public static async Task<ListingResponse> CreateAndPublishListingAsync(
        ApiClient sellerClient, string title = "Test Listing")
    {
        var categoryId = await GetAnyCategoryIdAsync(sellerClient);

        var createResponse = await sellerClient.PostAsync("/api/listings", new CreateListingCommand(
            Guid.Empty, categoryId, title, "A listing created for a test.", 50m, "EUR", GearCondition.Good, "Test City"));
        createResponse.EnsureSuccessStatusCode();
        var listing = (await createResponse.Content.ReadFromJsonAsync<ListingResponse>())!;

        var publishResponse = await sellerClient.PostAsync($"/api/listings/{listing.Id}/publish");
        publishResponse.EnsureSuccessStatusCode();

        return listing;
    }
}
