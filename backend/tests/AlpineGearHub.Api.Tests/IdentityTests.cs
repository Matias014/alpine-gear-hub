using System.Net;
using System.Net.Http.Json;
using AlpineGearHub.Api.Tests.Helpers;
using AlpineGearHub.Identity.Application.Commands.ConfirmPasswordReset;
using AlpineGearHub.Identity.Application.Commands.Login;
using AlpineGearHub.Identity.Application.Commands.RefreshToken;
using AlpineGearHub.Identity.Application.Commands.Register;
using AlpineGearHub.Identity.Application.Commands.RequestPasswordReset;
using AlpineGearHub.Identity.Application.DTOs;
using AlpineGearHub.Identity.Application.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

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

    private string GetLastPasswordResetToken(string email) =>
        ((CapturingEmailSender)factory.Services.GetRequiredService<IEmailSender>()).GetLastResetToken(email);

    [Fact]
    public async Task ForgotPassword_WithRegisteredEmail_ReturnsNoContentAndSendsAResetToken()
    {
        var client = new ApiClient(factory.CreateClient());
        var email = $"{Guid.NewGuid():N}@test.local";
        await client.PostAsync("/api/auth/register", new RegisterCommand("Forgot Password User", email, "Password1!"));

        var response = await client.PostAsync("/api/auth/forgot-password", new RequestPasswordResetCommand(email));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        GetLastPasswordResetToken(email).Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ForgotPassword_WithUnregisteredEmail_StillReturnsNoContent()
    {
        // Same response whether the account exists or not - otherwise this endpoint becomes a
        // free "does this email have an account" oracle for anyone probing it.
        var client = new ApiClient(factory.CreateClient());

        var response = await client.PostAsync("/api/auth/forgot-password",
            new RequestPasswordResetCommand($"{Guid.NewGuid():N}@nobody.local"));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_AllowsLoginWithNewPasswordAndRejectsTheOldOne()
    {
        var client = new ApiClient(factory.CreateClient());
        var email = $"{Guid.NewGuid():N}@test.local";
        await client.PostAsync("/api/auth/register", new RegisterCommand("Reset Password User", email, "Password1!"));
        await client.PostAsync("/api/auth/forgot-password", new RequestPasswordResetCommand(email));
        var token = GetLastPasswordResetToken(email);

        var resetResponse = await client.PostAsync("/api/auth/reset-password",
            new ConfirmPasswordResetCommand(token, "NewPassword2@"));
        resetResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var newLogin = await client.PostAsync("/api/auth/login", new LoginCommand(email, "NewPassword2@"));
        newLogin.StatusCode.Should().Be(HttpStatusCode.OK);

        var oldLogin = await client.PostAsync("/api/auth/login", new LoginCommand(email, "Password1!"));
        oldLogin.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_RevokesExistingRefreshTokens()
    {
        var client = new ApiClient(factory.CreateClient());
        var email = $"{Guid.NewGuid():N}@test.local";
        var registerResponse = await client.PostAsync("/api/auth/register", new RegisterCommand("Revoke Sessions User", email, "Password1!"));
        var original = (await registerResponse.Content.ReadFromJsonAsync<AuthResponse>())!;

        await client.PostAsync("/api/auth/forgot-password", new RequestPasswordResetCommand(email));
        var token = GetLastPasswordResetToken(email);
        await client.PostAsync("/api/auth/reset-password", new ConfirmPasswordResetCommand(token, "NewPassword2@"));

        var refreshResponse = await client.PostAsync("/api/auth/refresh", new RefreshTokenCommand(original.RefreshToken));

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResetPassword_ReusingAnAlreadyUsedToken_ReturnsUnauthorized()
    {
        var client = new ApiClient(factory.CreateClient());
        var email = $"{Guid.NewGuid():N}@test.local";
        await client.PostAsync("/api/auth/register", new RegisterCommand("Reuse Token User", email, "Password1!"));
        await client.PostAsync("/api/auth/forgot-password", new RequestPasswordResetCommand(email));
        var token = GetLastPasswordResetToken(email);
        await client.PostAsync("/api/auth/reset-password", new ConfirmPasswordResetCommand(token, "NewPassword2@"));

        var response = await client.PostAsync("/api/auth/reset-password",
            new ConfirmPasswordResetCommand(token, "AnotherPassword3#"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResetPassword_WithGarbageToken_ReturnsUnauthorized()
    {
        var client = new ApiClient(factory.CreateClient());

        var response = await client.PostAsync("/api/auth/reset-password",
            new ConfirmPasswordResetCommand("not-a-real-token", "NewPassword2@"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResetPassword_WeakNewPassword_ReturnsUnprocessableEntity()
    {
        var client = new ApiClient(factory.CreateClient());
        var email = $"{Guid.NewGuid():N}@test.local";
        await client.PostAsync("/api/auth/register", new RegisterCommand("Weak Reset User", email, "Password1!"));
        await client.PostAsync("/api/auth/forgot-password", new RequestPasswordResetCommand(email));
        var token = GetLastPasswordResetToken(email);

        var response = await client.PostAsync("/api/auth/reset-password", new ConfirmPasswordResetCommand(token, "weak"));

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task RequestingASecondReset_InvalidatesTheFirstToken()
    {
        var client = new ApiClient(factory.CreateClient());
        var email = $"{Guid.NewGuid():N}@test.local";
        await client.PostAsync("/api/auth/register", new RegisterCommand("Second Reset User", email, "Password1!"));

        await client.PostAsync("/api/auth/forgot-password", new RequestPasswordResetCommand(email));
        var firstToken = GetLastPasswordResetToken(email);
        await client.PostAsync("/api/auth/forgot-password", new RequestPasswordResetCommand(email));

        var response = await client.PostAsync("/api/auth/reset-password",
            new ConfirmPasswordResetCommand(firstToken, "NewPassword2@"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
