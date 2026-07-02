using AlpineGearHub.Chat.Application.Commands.SendMessage;
using AlpineGearHub.Chat.Application.Commands.StartConversation;
using AlpineGearHub.Chat.Application.Interfaces;
using AlpineGearHub.Chat.Domain.Repositories;
using AlpineGearHub.Chat.Infrastructure.Data;
using AlpineGearHub.Chat.Infrastructure.Repositories;
using AlpineGearHub.Chat.Infrastructure.Services;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlpineGearHub.Chat.Infrastructure;

public static class ChatModule
{
    public static IServiceCollection AddChatModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ChatDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IChatNotifier, SignalRChatNotifier>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(StartConversationCommand).Assembly));

        services.AddScoped<IValidator<StartConversationCommand>, StartConversationCommandValidator>();
        services.AddScoped<IValidator<SendMessageCommand>, SendMessageCommandValidator>();

        services.AddHealthChecks().AddDbContextCheck<ChatDbContext>("chat-db");

        return services;
    }

    public static void ApplyChatMigrations(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        scope.ServiceProvider
            .GetRequiredService<ChatDbContext>()
            .Database.Migrate();
    }
}
