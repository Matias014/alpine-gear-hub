using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AlpineGearHub.Api.Endpoints;
using AlpineGearHub.Api.Middleware;
using AlpineGearHub.Chat.Infrastructure;
using AlpineGearHub.Chat.Infrastructure.Hubs;
using AlpineGearHub.Identity.Domain.Entities;
using AlpineGearHub.Identity.Domain.Enums;
using AlpineGearHub.Identity.Infrastructure;
using AlpineGearHub.Identity.Infrastructure.Data;
using AlpineGearHub.Listings.Domain.Entities;
using AlpineGearHub.Listings.Infrastructure;
using AlpineGearHub.Listings.Infrastructure.Data;
using AlpineGearHub.Moderation.Infrastructure;
using AlpineGearHub.Promotions.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Serilog;

// Bootstrap logger so I still get logs if something blows up before the host's own logger is ready.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    RunApp(args);
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    // HostAbortedException is how WebApplicationFactory<Program> (our integration tests) grabs the
    // builder without actually starting the host - swallowing it here would break that mechanism.
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

return;

void RunApp(string[] runArgs)
{
var builder = WebApplication.CreateBuilder(runArgs);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console());

// ── JSON (enums as strings in request/response bodies) ───────────────────────
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// ── API docs ──────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "AlpineGearHub API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
    });
    options.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
            []
        }
    });
});

// ── JWT Authentication ────────────────────────────────────────────────────────
var jwtSection = builder.Configuration.GetSection("Jwt");
var secret = jwtSection["Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret is missing from configuration.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Map JWT "sub" → ClaimTypes.NameIdentifier (default is false since .NET 8)
        options.MapInboundClaims = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ClockSkew = TimeSpan.Zero,
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(token) && path.StartsWithSegments("/hubs"))
                    context.Token = token;
                return Task.CompletedTask;
            },
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireModerator", p =>
        p.RequireRole(UserRole.Moderator.ToString(), UserRole.Admin.ToString()));
    options.AddPolicy("RequireAdmin", p =>
        p.RequireRole(UserRole.Admin.ToString()));
});

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(
                builder.Configuration["Cors:AllowedOrigins"] ?? "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// ── SignalR ───────────────────────────────────────────────────────────────────
builder.Services.AddSignalR();

// ── Modules ───────────────────────────────────────────────────────────────────
builder.Services
    .AddIdentityModule(builder.Configuration)
    .AddListingsModule(builder.Configuration)
    .AddChatModule(builder.Configuration)
    .AddModerationModule(builder.Configuration)
    .AddPromotionsModule(builder.Configuration);

// ── Exception handling ────────────────────────────────────────────────────────
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

app.UseSerilogRequestLogging();

// ── Migrations & seed ─────────────────────────────────────────────────────────
app.Services.ApplyIdentityMigrations();
app.Services.ApplyListingsMigrations();
app.Services.ApplyChatMigrations();
app.Services.ApplyModerationMigrations();
app.Services.ApplyPromotionsMigrations();
app.Services.EnsureStorageBucketExistsAsync(app.Configuration).GetAwaiter().GetResult();

using (var scope = app.Services.CreateScope())
{
    var identityDb = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

    // Dev/test-only: a hardcoded admin account is a real backdoor once the source is public, so
    // this never runs outside Development. Any real deployment needs its own admin bootstrap
    // (e.g. a one-off script or direct DB insert) rather than a well-known default login.
    if (app.Environment.IsDevelopment() && !identityDb.Users.Any(u => u.Role == UserRole.Admin))
    {
        var adminEmail = app.Configuration["Seed:AdminEmail"] ?? "admin@alpinegearhub.local";
        var adminPassword = app.Configuration["Seed:AdminPassword"] ?? "Admin1234!";
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var passwordHash = hasher.HashPassword(null!, adminPassword);
        var admin = User.Create(adminEmail, "System Admin", passwordHash, UserRole.Admin);
        identityDb.Users.Add(admin);
        identityDb.SaveChanges();
    }

    var listingsDb = scope.ServiceProvider.GetRequiredService<ListingsDbContext>();
    if (!listingsDb.Categories.Any())
    {
        var categories = new[]
        {
            ("Ropes", "ropes"),
            ("Harnesses", "harnesses"),
            ("Helmets", "helmets"),
            ("Carabiners & Quickdraws", "carabiners-quickdraws"),
            ("Ice Axes & Crampons", "ice-axes-crampons"),
            ("Boots & Shoes", "boots-shoes"),
            ("Backpacks", "backpacks"),
            ("Tents & Shelters", "tents-shelters"),
            ("Clothing", "clothing"),
            ("Other", "other"),
        };

        foreach (var (name, slug) in categories)
            listingsDb.Categories.Add(Category.Create(name, slug));

        listingsDb.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

// Wired this up to actually ping Postgres/Redis instead of always saying "healthy" -
// caught myself relying on the old hardcoded version during earlier phases' testing.
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var payload = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
            }),
        });
        await context.Response.WriteAsync(payload);
    },
})
   .WithTags("Health")
   .AllowAnonymous();

app.MapGroup("/api/auth")
   .WithTags("Auth")
   .MapAuthEndpoints();

app.MapGroup("/api/categories")
   .WithTags("Categories")
   .MapCategoryEndpoints();

app.MapGroup("/api/listings")
   .WithTags("Listings")
   .MapListingEndpoints();

app.MapGroup("/api/chat")
   .WithTags("Chat")
   .MapChatEndpoints();

app.MapHub<ChatHub>("/hubs/chat");

app.MapGroup("/api/moderation")
   .WithTags("Moderation")
   .MapModerationEndpoints();

app.MapGroup("/api/promotions")
   .WithTags("Promotions")
   .MapPromotionsEndpoints();

app.Run();
}

public partial class Program { }
