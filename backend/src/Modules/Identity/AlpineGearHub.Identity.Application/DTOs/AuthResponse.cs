namespace AlpineGearHub.Identity.Application.DTOs;

public record AuthResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    string FullName,
    string Email,
    string Role
);
