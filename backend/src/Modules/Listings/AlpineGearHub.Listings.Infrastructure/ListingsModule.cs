using AlpineGearHub.Listings.Application.Commands.CreateListing;
using AlpineGearHub.Listings.Application.Interfaces;
using AlpineGearHub.Listings.Domain.Repositories;
using AlpineGearHub.Listings.Infrastructure.Data;
using AlpineGearHub.Listings.Infrastructure.Repositories;
using AlpineGearHub.Listings.Infrastructure.Services;
using Amazon.S3;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlpineGearHub.Listings.Infrastructure;

public static class ListingsModule
{
    public static IServiceCollection AddListingsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ListingsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        var storageEndpoint = configuration["Storage:Endpoint"] ?? "http://localhost:9000";
        var storageAccessKey = configuration["Storage:AccessKey"] ?? "minioadmin";
        var storageSecretKey = configuration["Storage:SecretKey"] ?? "minioadmin";

        services.AddSingleton<IAmazonS3>(_ =>
        {
            var config = new AmazonS3Config
            {
                ServiceURL = storageEndpoint,
                ForcePathStyle = true,
            };
            return new AmazonS3Client(storageAccessKey, storageSecretKey, config);
        });

        services.AddStackExchangeRedisCache(options =>
            options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379");

        services.AddScoped<IFileStorage, MinioFileStorage>();
        services.AddScoped<ICacheService, RedisCacheService>();
        services.AddScoped<IListingRepository, ListingRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(CreateListingCommand).Assembly));

        services.AddScoped<IValidator<CreateListingCommand>, CreateListingCommandValidator>();

        return services;
    }

    public static void ApplyListingsMigrations(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        scope.ServiceProvider
            .GetRequiredService<ListingsDbContext>()
            .Database.Migrate();
    }
}
