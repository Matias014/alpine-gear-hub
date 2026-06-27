using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace AlpineGearHub.Api.Tests;

public sealed class AlpineGearHubApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:18")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting(
            "ConnectionStrings:DefaultConnection",
            _postgres.GetConnectionString());

        builder.UseSetting("ConnectionStrings:Redis", "localhost:6379");

        builder.UseSetting("Jwt:Secret", "test-secret-key-that-is-long-enough-32chars!");
        builder.UseSetting("Jwt:Issuer", "alpinegearhub-api");
        builder.UseSetting("Jwt:Audience", "alpinegearhub-client");

        builder.UseSetting("Storage:Endpoint", "http://localhost:9000");
        builder.UseSetting("Storage:AccessKey", "minioadmin");
        builder.UseSetting("Storage:SecretKey", "minioadmin");
        builder.UseSetting("Storage:BucketName", "alpine-gear-hub");
        builder.UseSetting("Storage:PublicBaseUrl", "http://localhost:9000");
    }

    public async Task InitializeAsync() => await _postgres.StartAsync();

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}
