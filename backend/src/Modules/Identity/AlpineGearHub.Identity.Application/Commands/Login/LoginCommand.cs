using AlpineGearHub.Identity.Application.DTOs;
using MediatR;

namespace AlpineGearHub.Identity.Application.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;
