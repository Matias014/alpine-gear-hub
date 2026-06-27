using AlpineGearHub.Identity.Application.DTOs;
using MediatR;

namespace AlpineGearHub.Identity.Application.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResponse>;
