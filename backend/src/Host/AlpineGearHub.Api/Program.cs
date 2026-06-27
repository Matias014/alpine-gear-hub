using System.Text;
using AlpineGearHub.Api.Endpoints;
using AlpineGearHub.Api.Middleware;
using AlpineGearHub.Chat.Infrastructure;
using AlpineGearHub.Identity.Domain.Entities;
using AlpineGearHub.Identity.Domain.Enums;
using AlpineGearHub.Identity.Infrastructure;
using AlpineGearHub.Identity.Infrastructure.Data;
using AlpineGearHub.Listings.Infrastructure;
using AlpineGearHub.Moderation.Infrastructure;
using AlpineGearHub.Promotions.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

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
    .AddListingsModule()
    .AddChatModule()
    .AddModerationModule()
    .AddPromotionsModule();

// ── Exception handling ────────────────────────────────────────────────────────
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Migrations & seed ─────────────────────────────────────────────────────────
app.Services.ApplyIdentityMigrations();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    if (!db.Users.Any(u => u.Role == UserRole.Admin))
    {
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        var passwordHash = hasher.HashPassword(null!, "Admin1234!");
        var admin = User.Create("admin@alpinegearhub.local", "System Admin", passwordHash, UserRole.Admin);
        db.Users.Add(admin);
        db.SaveChanges();
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

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
   .WithTags("Health")
   .AllowAnonymous();

app.MapGroup("/api/auth")
   .WithTags("Auth")
   .MapAuthEndpoints();

app.Run();

public partial class Program { }
