using AlpineGearHub.Identity.Application.Commands.ConfirmEmail;
using AlpineGearHub.Identity.Application.Commands.ConfirmPasswordReset;
using AlpineGearHub.Identity.Application.Commands.Login;
using AlpineGearHub.Identity.Application.Commands.Logout;
using AlpineGearHub.Identity.Application.Commands.RefreshToken;
using AlpineGearHub.Identity.Application.Commands.Register;
using AlpineGearHub.Identity.Application.Commands.RequestPasswordReset;
using AlpineGearHub.Identity.Application.Commands.ResendEmailConfirmation;
using AlpineGearHub.Identity.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace AlpineGearHub.Api.Endpoints;

public static class AuthEndpoints
{
    // Scoped to /api/auth so the browser only ever attaches it to the handful of endpoints that
    // actually need it, not every API call.
    private const string RefreshTokenCookieName = "refreshToken";
    private const string RefreshTokenCookiePath = "/api/auth";

    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/register", Register)
            .AllowAnonymous()
            .Produces<ClientAuthResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
            .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)
            .WithSummary("Register a new account");

        group.MapPost("/login", Login)
            .AllowAnonymous()
            .Produces<ClientAuthResponse>()
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .WithSummary("Login and receive tokens");

        group.MapPost("/refresh", Refresh)
            .AllowAnonymous()
            .Produces<ClientAuthResponse>()
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .WithSummary("Refresh access token");

        group.MapPost("/logout", Logout)
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent)
            .WithSummary("Log out and revoke the current refresh token");

        group.MapPost("/forgot-password", ForgotPassword)
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)
            .WithSummary("Request a password reset email");

        group.MapPost("/reset-password", ResetPassword)
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)
            .WithSummary("Reset a password using a reset token");

        group.MapPost("/confirm-email", ConfirmEmail)
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .WithSummary("Confirm an account's email using a confirmation token");

        group.MapPost("/resend-confirmation", ResendConfirmation)
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)
            .WithSummary("Resend the email confirmation link");

        return group;
    }

    private static async Task<IResult> Register(
        [FromBody] RegisterCommand command,
        ISender sender,
        HttpContext httpContext,
        IHostEnvironment env,
        IConfiguration configuration,
        CancellationToken ct)
    {
        var response = await sender.Send(command, ct);
        SetRefreshTokenCookie(httpContext, response.RefreshToken, env, configuration);
        return Results.Created("/api/auth/me", ClientAuthResponse.From(response));
    }

    private static async Task<IResult> Login(
        [FromBody] LoginCommand command,
        ISender sender,
        HttpContext httpContext,
        IHostEnvironment env,
        IConfiguration configuration,
        CancellationToken ct)
    {
        var response = await sender.Send(command, ct);
        SetRefreshTokenCookie(httpContext, response.RefreshToken, env, configuration);
        return Results.Ok(ClientAuthResponse.From(response));
    }

    private static async Task<IResult> Refresh(
        ISender sender,
        HttpContext httpContext,
        IHostEnvironment env,
        IConfiguration configuration,
        CancellationToken ct)
    {
        // The raw token never appears in a request body anymore - it travels only as the
        // httpOnly cookie set below, so a script reading a fetch response (e.g. via XSS) can't
        // get at it the way it could when it was also echoed back in the JSON body.
        var refreshToken = httpContext.Request.Cookies[RefreshTokenCookieName];
        var response = await sender.Send(new RefreshTokenCommand(refreshToken ?? string.Empty), ct);
        SetRefreshTokenCookie(httpContext, response.RefreshToken, env, configuration);
        return Results.Ok(ClientAuthResponse.From(response));
    }

    private static async Task<IResult> Logout(
        ISender sender,
        HttpContext httpContext,
        IHostEnvironment env,
        CancellationToken ct)
    {
        var refreshToken = httpContext.Request.Cookies[RefreshTokenCookieName];
        await sender.Send(new LogoutCommand(refreshToken), ct);
        ClearRefreshTokenCookie(httpContext, env);
        return Results.NoContent();
    }

    private static async Task<IResult> ForgotPassword(
        [FromBody] RequestPasswordResetCommand command,
        ISender sender,
        CancellationToken ct)
    {
        await sender.Send(command, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ResetPassword(
        [FromBody] ConfirmPasswordResetCommand command,
        ISender sender,
        CancellationToken ct)
    {
        await sender.Send(command, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ConfirmEmail(
        [FromBody] ConfirmEmailCommand command,
        ISender sender,
        CancellationToken ct)
    {
        await sender.Send(command, ct);
        return Results.NoContent();
    }

    private static async Task<IResult> ResendConfirmation(
        [FromBody] ResendEmailConfirmationCommand command,
        ISender sender,
        CancellationToken ct)
    {
        await sender.Send(command, ct);
        return Results.NoContent();
    }

    private static void SetRefreshTokenCookie(
        HttpContext httpContext, string refreshToken, IHostEnvironment env, IConfiguration configuration)
    {
        var expiryDays = int.Parse(configuration["Jwt:RefreshTokenExpiryDays"] ?? "7");

        httpContext.Response.Cookies.Append(RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            // Both the local dev server and the docker-compose "production-like" setup run over
            // plain http://localhost (see docker-compose.yml's ASPNETCORE_ENVIRONMENT=Development) -
            // a Secure cookie simply wouldn't be stored there. A real deployment behind HTTPS needs
            // ASPNETCORE_ENVIRONMENT=Production for this to actually apply.
            Secure = !env.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            Path = RefreshTokenCookiePath,
            Expires = DateTimeOffset.UtcNow.AddDays(expiryDays),
        });
    }

    private static void ClearRefreshTokenCookie(HttpContext httpContext, IHostEnvironment env)
    {
        httpContext.Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = !env.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            Path = RefreshTokenCookiePath,
        });
    }
}

// The client-facing shape of AuthResponse - deliberately omits RefreshToken, which now only ever
// travels as an httpOnly cookie (see SetRefreshTokenCookie above), never in a JSON body a script
// could read.
public record ClientAuthResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string FullName,
    string Email,
    string Role)
{
    public static ClientAuthResponse From(AuthResponse auth) =>
        new(auth.AccessToken, auth.AccessTokenExpiresAt, auth.FullName, auth.Email, auth.Role);
}
