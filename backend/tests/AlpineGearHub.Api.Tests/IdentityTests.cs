using System.Net;
using System.Net.Http.Json;
using AlpineGearHub.Api.Endpoints;
using AlpineGearHub.Api.Tests.Helpers;
using AlpineGearHub.Identity.Application.Commands.ConfirmEmail;
using AlpineGearHub.Identity.Application.Commands.ConfirmPasswordReset;
using AlpineGearHub.Identity.Application.Commands.Login;
using AlpineGearHub.Identity.Application.Commands.Register;
using AlpineGearHub.Identity.Application.Commands.RequestPasswordReset;
using AlpineGearHub.Identity.Application.Commands.ResendEmailConfirmation;
using AlpineGearHub.Identity.Application.DTOs;
using AlpineGearHub.Identity.Application.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace AlpineGearHub.Api.Tests;

[Collection(DatabaseCollection.Name)]
public sealed class IdentityTests(AlpineGearHubApiFactory factory)
{
    [Fact]
    public async Task Register_ReturnsCreatedWithAnAccessTokenAndSetsARefreshCookie()
    {
        var client = new ApiClient(factory.CreateClient());
        var email = $"{Guid.NewGuid():N}@test.local";

        var response = await client.PostAsync("/api/auth/register", new RegisterCommand("New User", email, "Password1!"));

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var auth = await response.Content.ReadFromJsonAsync<ClientAuthResponse>();
        auth!.AccessToken.Should().NotBeNullOrWhiteSpace();
        auth.Email.Should().Be(email);
        auth.Role.Should().Be("Member");
        // The refresh token itself must never appear in the body - only as the httpOnly cookie.
        (await response.Content.ReadAsStringAsync()).Should().NotContain("refreshToken");
        ApiClient.GetCookieValue(response, "refreshToken").Should().NotBeNullOrWhiteSpace();
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
    public async Task Login_WithCorrectCredentials_ReturnsAnAccessTokenAndSetsARefreshCookie()
    {
        var client = new ApiClient(factory.CreateClient());
        var email = $"{Guid.NewGuid():N}@test.local";
        await client.PostAsync("/api/auth/register", new RegisterCommand("Login User", email, "Password1!"));

        var response = await client.PostAsync("/api/auth/login", new LoginCommand(email, "Password1!"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await response.Content.ReadFromJsonAsync<ClientAuthResponse>();
        auth!.Email.Should().Be(email);
        ApiClient.GetCookieValue(response, "refreshToken").Should().NotBeNullOrWhiteSpace();
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
    public async Task Refresh_WithValidCookie_ReturnsNewTokenPairAndRotatesTheCookie()
    {
        var client = new ApiClient(factory.CreateClient());
        var email = $"{Guid.NewGuid():N}@test.local";
        var registerResponse = await client.PostAsync("/api/auth/register", new RegisterCommand("Refresh User", email, "Password1!"));
        var originalCookie = ApiClient.GetCookieValue(registerResponse, "refreshToken");

        // No body needed - the client's cookie jar already carries the one Register just set.
        var response = await client.PostAsync("/api/auth/refresh");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().NotContain("refreshToken");
        ApiClient.GetCookieValue(response, "refreshToken").Should().NotBe(originalCookie);
    }

    [Fact]
    public async Task Refresh_ReusingARotatedCookie_ReturnsUnauthorized()
    {
        var client = new ApiClient(factory.CreateClient());
        var email = $"{Guid.NewGuid():N}@test.local";
        var registerResponse = await client.PostAsync("/api/auth/register", new RegisterCommand("Rotate User", email, "Password1!"));
        var originalCookie = ApiClient.GetCookieValue(registerResponse, "refreshToken")!;

        // Refreshing once rotates (and revokes) the original token via the client's own jar.
        await client.PostAsync("/api/auth/refresh");

        // Replay the now-superseded cookie explicitly, via a fresh client with an empty jar (so it
        // doesn't also send whatever the first client's jar rotated to), to prove it's rejected.
        var staleClient = new ApiClient(factory.CreateClient());
        var response = await staleClient.PostWithCookieAsync("/api/auth/refresh", "refreshToken", originalCookie);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithGarbageCookie_ReturnsUnauthorized()
    {
        var client = new ApiClient(factory.CreateClient());

        var response = await client.PostWithCookieAsync("/api/auth/refresh", "refreshToken", "not-a-real-token");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithNoCookieAtAll_ReturnsUnauthorized()
    {
        var client = new ApiClient(factory.CreateClient());

        var response = await client.PostAsync("/api/auth/refresh");

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
        await client.PostAsync("/api/auth/register", new RegisterCommand("Revoke Sessions User", email, "Password1!"));

        await client.PostAsync("/api/auth/forgot-password", new RequestPasswordResetCommand(email));
        var token = GetLastPasswordResetToken(email);
        await client.PostAsync("/api/auth/reset-password", new ConfirmPasswordResetCommand(token, "NewPassword2@"));

        // Reset doesn't touch the cookie, so the client's jar still carries the pre-reset one -
        // which should now be revoked.
        var refreshResponse = await client.PostAsync("/api/auth/refresh");

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

    private string GetLastEmailConfirmationToken(string email) =>
        ((CapturingEmailSender)factory.Services.GetRequiredService<IEmailSender>()).GetLastConfirmationToken(email);

    [Fact]
    public async Task Register_SendsAnEmailConfirmationToken()
    {
        var client = new ApiClient(factory.CreateClient());
        var email = $"{Guid.NewGuid():N}@test.local";

        await client.PostAsync("/api/auth/register", new RegisterCommand("Confirm Email User", email, "Password1!"));

        GetLastEmailConfirmationToken(email).Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithUnconfirmedEmail_StillSucceeds()
    {
        // Email confirmation gates publishing listings and messaging (see the
        // "RequireConfirmedEmail" policy), not login itself - registering shouldn't strand anyone
        // who hasn't clicked the link yet.
        var client = new ApiClient(factory.CreateClient());
        var email = $"{Guid.NewGuid():N}@test.local";
        await client.PostAsync("/api/auth/register", new RegisterCommand("Unconfirmed Login User", email, "Password1!"));

        var response = await client.PostAsync("/api/auth/login", new LoginCommand(email, "Password1!"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ConfirmEmail_WithValidToken_Succeeds()
    {
        var client = new ApiClient(factory.CreateClient());
        var email = $"{Guid.NewGuid():N}@test.local";
        await client.PostAsync("/api/auth/register", new RegisterCommand("Confirm Success User", email, "Password1!"));
        var token = GetLastEmailConfirmationToken(email);

        var response = await client.PostAsync("/api/auth/confirm-email", new ConfirmEmailCommand(token));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ConfirmEmail_ReusingAnAlreadyUsedToken_ReturnsUnauthorized()
    {
        var client = new ApiClient(factory.CreateClient());
        var email = $"{Guid.NewGuid():N}@test.local";
        await client.PostAsync("/api/auth/register", new RegisterCommand("Reuse Confirm User", email, "Password1!"));
        var token = GetLastEmailConfirmationToken(email);
        await client.PostAsync("/api/auth/confirm-email", new ConfirmEmailCommand(token));

        var response = await client.PostAsync("/api/auth/confirm-email", new ConfirmEmailCommand(token));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ConfirmEmail_WithGarbageToken_ReturnsUnauthorized()
    {
        var client = new ApiClient(factory.CreateClient());

        var response = await client.PostAsync("/api/auth/confirm-email", new ConfirmEmailCommand("not-a-real-token"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResendConfirmation_WithUnregisteredEmail_ReturnsNoContent()
    {
        // Same no-enumeration story as forgot-password.
        var client = new ApiClient(factory.CreateClient());

        var response = await client.PostAsync("/api/auth/resend-confirmation",
            new ResendEmailConfirmationCommand($"{Guid.NewGuid():N}@nobody.local"));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ResendConfirmation_SendsAFreshTokenThatInvalidatesTheOriginal()
    {
        var client = new ApiClient(factory.CreateClient());
        var email = $"{Guid.NewGuid():N}@test.local";
        await client.PostAsync("/api/auth/register", new RegisterCommand("Resend User", email, "Password1!"));
        var originalToken = GetLastEmailConfirmationToken(email);

        await client.PostAsync("/api/auth/resend-confirmation", new ResendEmailConfirmationCommand(email));
        var freshToken = GetLastEmailConfirmationToken(email);

        freshToken.Should().NotBe(originalToken);
        var confirmWithFresh = await client.PostAsync("/api/auth/confirm-email", new ConfirmEmailCommand(freshToken));
        confirmWithFresh.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var confirmWithOriginal = await client.PostAsync("/api/auth/confirm-email", new ConfirmEmailCommand(originalToken));
        confirmWithOriginal.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ResendConfirmation_ForAlreadyConfirmedAccount_DoesNotSendAnotherToken()
    {
        var client = new ApiClient(factory.CreateClient());
        var email = $"{Guid.NewGuid():N}@test.local";
        await client.PostAsync("/api/auth/register", new RegisterCommand("Already Confirmed User", email, "Password1!"));
        var token = GetLastEmailConfirmationToken(email);
        await client.PostAsync("/api/auth/confirm-email", new ConfirmEmailCommand(token));

        var response = await client.PostAsync("/api/auth/resend-confirmation", new ResendEmailConfirmationCommand(email));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        // No new email captured - still the one from registration.
        GetLastEmailConfirmationToken(email).Should().Be(token);
    }

    [Fact]
    public async Task Logout_RevokesTheRefreshCookie()
    {
        var client = new ApiClient(factory.CreateClient());
        var email = $"{Guid.NewGuid():N}@test.local";
        await client.PostAsync("/api/auth/register", new RegisterCommand("Logout User", email, "Password1!"));

        var logoutResponse = await client.PostAsync("/api/auth/logout");
        logoutResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // The client's jar still holds the (now-revoked) pre-logout cookie value.
        var refreshResponse = await client.PostAsync("/api/auth/refresh");
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_WithNoCookie_StillReturnsNoContent()
    {
        // Logging out with nothing to revoke isn't an error - there's no sensitive "you weren't
        // logged in" fact to protect, and the client just wants its (already-absent) session gone.
        var client = new ApiClient(factory.CreateClient());

        var response = await client.PostAsync("/api/auth/logout");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
