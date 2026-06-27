using AlpineGearHub.Identity.Application.Commands.Login;
using AlpineGearHub.Identity.Application.Commands.RefreshToken;
using AlpineGearHub.Identity.Application.Commands.Register;
using AlpineGearHub.Identity.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AlpineGearHub.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        group.MapPost("/register", Register)
            .AllowAnonymous()
            .Produces<AuthResponse>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status409Conflict)
            .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)
            .WithSummary("Register a new account");

        group.MapPost("/login", Login)
            .AllowAnonymous()
            .Produces<AuthResponse>()
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .WithSummary("Login and receive tokens");

        group.MapPost("/refresh", Refresh)
            .AllowAnonymous()
            .Produces<AuthResponse>()
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .WithSummary("Refresh access token");

        return group;
    }

    private static async Task<IResult> Register(
        [FromBody] RegisterCommand command,
        ISender sender,
        CancellationToken ct)
    {
        var response = await sender.Send(command, ct);
        return Results.Created("/api/auth/me", response);
    }

    private static async Task<IResult> Login(
        [FromBody] LoginCommand command,
        ISender sender,
        CancellationToken ct)
    {
        var response = await sender.Send(command, ct);
        return Results.Ok(response);
    }

    private static async Task<IResult> Refresh(
        [FromBody] RefreshTokenCommand command,
        ISender sender,
        CancellationToken ct)
    {
        var response = await sender.Send(command, ct);
        return Results.Ok(response);
    }
}
