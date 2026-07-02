using AlpineGearHub.Moderation.Application.Commands.CreateReport;
using AlpineGearHub.Moderation.Domain.Repositories;
using AlpineGearHub.Moderation.Infrastructure.Data;
using AlpineGearHub.Moderation.Infrastructure.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlpineGearHub.Moderation.Infrastructure;

public static class ModerationModule
{
    public static IServiceCollection AddModerationModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ModerationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IReportRepository, ReportRepository>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CreateReportCommand).Assembly));

        services.AddScoped<IValidator<CreateReportCommand>, CreateReportCommandValidator>();

        return services;
    }

    public static void ApplyModerationMigrations(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        scope.ServiceProvider
            .GetRequiredService<ModerationDbContext>()
            .Database.Migrate();
    }
}
