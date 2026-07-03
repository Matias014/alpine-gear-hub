using AlpineGearHub.Listings.Application.Commands.CreateListing;
using AlpineGearHub.Listings.Application.Interfaces;
using AlpineGearHub.Listings.Domain.Repositories;
using AlpineGearHub.Listings.Infrastructure.Data;
using AlpineGearHub.Listings.Infrastructure.Repositories;
using AlpineGearHub.Listings.Infrastructure.Services;
using Amazon.S3;
using Amazon.S3.Util;
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

        services.AddHealthChecks().AddDbContextCheck<ListingsDbContext>("listings-db");

        return services;
    }

    public static void ApplyListingsMigrations(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        scope.ServiceProvider
            .GetRequiredService<ListingsDbContext>()
            .Database.Migrate();
    }

    // Found out the hard way that MinIO doesn't come with the bucket pre-created, and even once
    // it exists it's private by default - image uploads and GetPublicUrl both silently assumed
    // otherwise, so nobody had actually driven this end to end before.
    public static async Task EnsureStorageBucketExistsAsync(this IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var s3 = scope.ServiceProvider.GetRequiredService<IAmazonS3>();
        var bucket = configuration["Storage:BucketName"] ?? "alpine-gear-hub";

        if (!await AmazonS3Util.DoesS3BucketExistV2Async(s3, bucket))
            await s3.PutBucketAsync(bucket);

        var publicReadPolicy = $$"""
            {
              "Version": "2012-10-17",
              "Statement": [
                {
                  "Effect": "Allow",
                  "Principal": "*",
                  "Action": ["s3:GetObject"],
                  "Resource": ["arn:aws:s3:::{{bucket}}/*"]
                }
              ]
            }
            """;

        await s3.PutBucketPolicyAsync(bucket, publicReadPolicy);
    }
}
