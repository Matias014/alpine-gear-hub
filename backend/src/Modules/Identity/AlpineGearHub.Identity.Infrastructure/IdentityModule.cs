using AlpineGearHub.Identity.Application.Behaviors;
using AlpineGearHub.Identity.Application.Commands.Register;
using AlpineGearHub.Identity.Application.Interfaces;
using AlpineGearHub.Identity.Domain.Entities;
using AlpineGearHub.Identity.Domain.Repositories;
using AlpineGearHub.Identity.Infrastructure.Data;
using AlpineGearHub.Identity.Infrastructure.HealthChecks;
using AlpineGearHub.Identity.Infrastructure.Repositories;
using AlpineGearHub.Identity.Infrastructure.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace AlpineGearHub.Identity.Infrastructure;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis") ?? "localhost:6379"));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<ILoginRateLimiter, RedisLoginRateLimiter>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(RegisterCommand).Assembly));

        services.AddScoped<IValidator<RegisterCommand>, RegisterCommandValidator>();

        services.AddTransient(
            typeof(IPipelineBehavior<,>),
            typeof(ValidationBehavior<,>));

        services.AddHealthChecks()
            .AddDbContextCheck<IdentityDbContext>("identity-db")
            .AddCheck<RedisHealthCheck>("redis");

        return services;
    }

    public static void ApplyIdentityMigrations(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        scope.ServiceProvider
            .GetRequiredService<IdentityDbContext>()
            .Database.Migrate();
    }
}
