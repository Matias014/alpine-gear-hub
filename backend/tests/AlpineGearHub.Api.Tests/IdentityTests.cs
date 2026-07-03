using System.Net;
using System.Net.Http.Json;
using AlpineGearHub.Api.Tests.Helpers;
using AlpineGearHub.Identity.Application.Commands.Login;
using AlpineGearHub.Identity.Application.Commands.RefreshToken;
using AlpineGearHub.Identity.Application.Commands.Register;
using AlpineGearHub.Identity.Application.DTOs;
using FluentAssertions;

namespace AlpineGearHub.Api.Tests;

[Collection(DatabaseCollection.Name)]
public sealed class IdentityTests(AlpineGearHubApiFactory factory)
{
    [Fact]
    public async Task Register_ReturnsCreatedWithTokens()
    {
        var client = new ApiClient(factory.CreateClient());
        var email = $"{Guid.NewGuid():N}@test.local";

        var response = await client.PostAsync("/api/auth/register", new RegisterCommand("New User", email, "Password1!"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        auth!.AccessToken.Should().NotBeNullOrWhiteSpace();
        auth.Email.Should().Be(email);
        auth.Role.Should().Be("Member");
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        var client = new ApiClient(factory.CreateClient());
        var email = $"{Guid.NewGuid():N}@test.local";
        await client.PostAsync("/api/auth/register", new RegisterCommand("First", email, "Password1!"));

        var response = await client.PostAsync("/api/auth/register", new RegisterCommand("Second", email, "Password1!"));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WeakPassword_ReturnsUnprocessableEntity()
    {
        var client = new ApiClient(factory.CreateClient());

        var response = await client.PostAsync("/api/auth/register",
            new RegisterCommand("Weak Password", $"{Guid.NewGuid():N}@test.local", "weak"));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task Login_WithCorrectCredentials_ReturnsTokens()
    {
        var client = new ApiClient(factory.CreateClient());
        var email = $"{Guid.NewGuid():N}@test.local";
        await client.PostAsync("/api/auth/register", new RegisterCommand("Login User", email, "Password1!"));

        var response = await client.PostAsync("/api/auth/login", new LoginCommand(email, "Password1!"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        auth!.Email.Should().Be(email);
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var client = new ApiClient(factory.CreateClient());
        var email = $"{Guid.NewGuid():N}@test.local";
        await client.PostAsync("/api/auth/register", new RegisterCommand("Wrong Password", email, "Password1!"));

        var response = await client.PostAsync("/api/auth/login", new LoginCommand(email, "NotThePassword1!"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_NonexistentEmail_ReturnsUnauthorized()
    {
        var client = new ApiClient(factory.CreateClient());

        var response = await client.PostAsync("/api/auth/login",
            new LoginCommand($"{Guid.NewGuid():N}@nobody.local", "Password1!"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithValidToken_ReturnsNewTokenPair()
    {
        var client = new ApiClient(factory.CreateClient());
        var email = $"{Guid.NewGuid():N}@test.local";
        var registerResponse = await client.PostAsync("/api/auth/register", new RegisterCommand("Refresh User", email, "Password1!"));
        var original = (await registerResponse.Content.ReadFromJsonAsync<AuthResponse>())!;

        var response = await client.PostAsync("/api/auth/refresh", new RefreshTokenCommand(original.RefreshToken));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshed = await response.Content.ReadFromJsonAsync<AuthResponse>();
        refreshed!.RefreshToken.Should().NotBe(original.RefreshToken);
    }

    [Fact]
    public async Task Refresh_ReusingRotatedToken_ReturnsUnauthorized()
    {
        var client = new ApiClient(factory.CreateClient());
        var email = $"{Guid.NewGuid():N}@test.local";
        var registerResponse = await client.PostAsync("/api/auth/register", new RegisterCommand("Rotate User", email, "Password1!"));
        var original = (await registerResponse.Content.ReadFromJsonAsync<AuthResponse>())!;

        // Refreshing once rotates and revokes the original token, so reusing it should now fail.
        await client.PostAsync("/api/auth/refresh", new RefreshTokenCommand(original.RefreshToken));
        var response = await client.PostAsync("/api/auth/refresh", new RefreshTokenCommand(original.RefreshToken));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithGarbageToken_ReturnsUnauthorized()
    {
        var client = new ApiClient(factory.CreateClient());

        var response = await client.PostAsync("/api/auth/refresh", new RefreshTokenCommand("not-a-real-token"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_AfterFiveFailedAttempts_LocksOutEvenTheCorrectPassword()
    {
        var client = new ApiClient(factory.CreateClient());
        var email = $"{Guid.NewGuid():N}@test.local";
        await client.PostAsync("/api/auth/register", new RegisterCommand("Locked Out", email, "Password1!"));

        for (var i = 0; i < 5; i++)
            await client.PostAsync("/api/auth/login", new LoginCommand(email, "WrongPassword!"));

        var response = await client.PostAsync("/api/auth/login", new LoginCommand(email, "Password1!"));

        response.StatusCode.Should().Be((HttpStatusCode)429);
    }
}
