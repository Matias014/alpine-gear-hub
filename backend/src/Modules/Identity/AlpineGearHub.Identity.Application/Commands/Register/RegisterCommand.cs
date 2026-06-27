using AlpineGearHub.Identity.Application.DTOs;
using MediatR;

namespace AlpineGearHub.Identity.Application.Commands.Register;

public record RegisterCommand(string FullName, string Email, string Password) : IRequest<AuthResponse>;
