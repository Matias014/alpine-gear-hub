using AlpineGearHub.Identity.Domain.Entities;
using AlpineGearHub.Identity.Domain.Repositories;
using AlpineGearHub.Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlpineGearHub.Identity.Infrastructure.Repositories;

public sealed class UserRepository(IdentityDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.PasswordResetTokens)
            .Include(u => u.EmailConfirmationTokens)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
        db.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.PasswordResetTokens)
            .Include(u => u.EmailConfirmationTokens)
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken ct = default) =>
        db.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.PasswordResetTokens)
            .Include(u => u.EmailConfirmationTokens)
            .FirstOrDefaultAsync(u => u.RefreshTokens.Any(rt => rt.TokenHash == tokenHash), ct);

    public Task<User?> GetByPasswordResetTokenHashAsync(string tokenHash, CancellationToken ct = default) =>
        db.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.PasswordResetTokens)
            .Include(u => u.EmailConfirmationTokens)
            .FirstOrDefaultAsync(u => u.PasswordResetTokens.Any(t => t.TokenHash == tokenHash), ct);

    public Task<User?> GetByEmailConfirmationTokenHashAsync(string tokenHash, CancellationToken ct = default) =>
        db.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.PasswordResetTokens)
            .Include(u => u.EmailConfirmationTokens)
            .FirstOrDefaultAsync(u => u.EmailConfirmationTokens.Any(t => t.TokenHash == tokenHash), ct);

    public async Task AddAsync(User user, CancellationToken ct = default) =>
        await db.Users.AddAsync(user, ct);

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        db.SaveChangesAsync(ct);
}
