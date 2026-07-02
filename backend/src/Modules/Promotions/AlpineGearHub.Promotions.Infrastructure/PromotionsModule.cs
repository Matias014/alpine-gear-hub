using AlpineGearHub.Promotions.Application.Commands.CreatePromotion;
using AlpineGearHub.Promotions.Application.Interfaces;
using AlpineGearHub.Promotions.Domain.Repositories;
using AlpineGearHub.Promotions.Infrastructure.Data;
using AlpineGearHub.Promotions.Infrastructure.Repositories;
using AlpineGearHub.Promotions.Infrastructure.Services;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlpineGearHub.Promotions.Infrastructure;

public static class PromotionsModule
{
    public static IServiceCollection AddPromotionsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PromotionsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IPromotionRepository, PromotionRepository>();
        services.AddScoped<IPaymentGateway, StripePaymentGateway>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CreatePromotionCommand).Assembly));

        services.AddScoped<IValidator<CreatePromotionCommand>, CreatePromotionCommandValidator>();

        services.AddHealthChecks().AddDbContextCheck<PromotionsDbContext>("promotions-db");

        return services;
    }

    public static void ApplyPromotionsMigrations(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        scope.ServiceProvider
            .GetRequiredService<PromotionsDbContext>()
            .Database.Migrate();
    }
}
