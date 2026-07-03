using AlpineGearHub.Api.Tests.Helpers;

namespace AlpineGearHub.Api.Tests;

[Collection(DatabaseCollection.Name)]
public sealed class HealthTests(AlpineGearHubApiFactory factory)
{
    private readonly ApiClient _client = new(factory.CreateClient());

    [Fact]
    public async Task HealthEndpoint_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        response.EnsureSuccessStatusCode();
    }
}
