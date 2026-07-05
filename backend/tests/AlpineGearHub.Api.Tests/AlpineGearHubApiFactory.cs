using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.Minio;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace AlpineGearHub.Api.Tests;

public sealed class AlpineGearHubApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:18")
        .Build();

    // Added this after CI failed - the app now connects to Redis eagerly at startup (login
    // rate limiting), and the hardcoded "localhost:6379" only worked on my machine because I
    // happen to have a real Redis running there via docker-compose. CI has nothing at that address.
    private readonly RedisContainer _redis = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .Build();

    // Added so the real image-upload path (previously only ever exercised manually against a
    // dev MinIO at localhost:9000) is actually covered by CI too - see ListingsTests.UploadImage_*.
    private readonly MinioContainer _minio = new MinioBuilder()
        .WithImage("minio/minio:latest")
        .WithUsername("minioadmin")
        .WithPassword("minioadmin")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting(
            "ConnectionStrings:DefaultConnection",
            _postgres.GetConnectionString());

        builder.UseSetting("ConnectionStrings:Redis", _redis.GetConnectionString());

        builder.UseSetting("Jwt:Secret", "test-secret-key-that-is-long-enough-32chars!");
        builder.UseSetting("Jwt:Issuer", "alpinegearhub-api");
        builder.UseSetting("Jwt:Audience", "alpinegearhub-client");

        var minioEndpoint = _minio.GetConnectionString();
        builder.UseSetting("Storage:Endpoint", minioEndpoint);
        builder.UseSetting("Storage:AccessKey", "minioadmin");
        builder.UseSetting("Storage:SecretKey", "minioadmin");
        builder.UseSetting("Storage:BucketName", "alpine-gear-hub");
        builder.UseSetting("Storage:PublicBaseUrl", minioEndpoint);

        // Pinning these explicitly rather than relying on appsettings.Development.json loading -
        // WebhookSigner.cs below needs to know the exact secret to sign test payloads against.
        builder.UseSetting("Stripe:SecretKey", "sk_test_placeholder");
        builder.UseSetting("Stripe:WebhookSecret", "whsec_test_secret_for_integration_tests");
    }

    public async Task InitializeAsync() =>
        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync(), _minio.StartAsync());

    public new async Task DisposeAsync()
    {
        await Task.WhenAll(_postgres.DisposeAsync().AsTask(), _redis.DisposeAsync().AsTask(), _minio.DisposeAsync().AsTask());
        await base.DisposeAsync();
    }
}
