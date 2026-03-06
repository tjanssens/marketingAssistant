using Discord.WebSocket;
using MarketingAssistant.Core.Interfaces;
using MarketingAssistant.Core.Models;
using MarketingAssistant.Discord.Builders;
using MarketingAssistant.Infrastructure.Data;
using MarketingAssistant.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MarketingAssistant.Discord.Handlers;

public class MessageHandler
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<MessageHandler> _logger;

    public MessageHandler(IServiceScopeFactory scopeFactory, ILogger<MessageHandler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task HandleAsync(SocketMessage message)
    {
        if (message.Author.IsBot) return;
        if (message is not SocketUserMessage userMessage) return;

        var content = userMessage.Content.Trim();
        if (string.IsNullOrEmpty(content)) return;

        _logger.LogInformation("Discord message from {User}: {Content}", message.Author.Username, content);

        using var typing = message.Channel.EnterTypingState();
        using var scope = _scopeFactory.CreateScope();

        var ai = scope.ServiceProvider.GetRequiredService<IAiBrainService>();
        var aggregator = scope.ServiceProvider.GetRequiredService<DataAggregator>();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            var context = await aggregator.GetAggregatedDataAsync();
            var answer = await ai.AnswerQuestionAsync(content, context);

            var conversation = new ConversationMessage
            {
                Timestamp = DateTime.UtcNow,
                Source = "discord",
                UserId = message.Author.Id.ToString(),
                UserMessage = content,
                AiResponse = answer
            };
            db.ConversationMessages.Add(conversation);
            await db.SaveChangesAsync();

            var embed = EmbedBuilders.BuildAnswerEmbed(content, answer);
            await message.Channel.SendMessageAsync(embed: embed, messageReference: new global::Discord.MessageReference(message.Id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Discord message");
            await message.Channel.SendMessageAsync("Er ging iets mis bij het verwerken van je vraag. Probeer het later opnieuw.");
        }
    }
}
