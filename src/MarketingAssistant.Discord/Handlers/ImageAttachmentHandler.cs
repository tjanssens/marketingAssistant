using Discord.WebSocket;
using MarketingAssistant.Core.Interfaces;
using MarketingAssistant.Core.Models;
using MarketingAssistant.Discord.Builders;
using MarketingAssistant.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MarketingAssistant.Discord.Handlers;

public class ImageAttachmentHandler
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ImageAttachmentHandler> _logger;

    private static readonly HashSet<string> SupportedMimeTypes = ["image/jpeg", "image/png", "image/gif", "image/webp"];

    public ImageAttachmentHandler(IServiceScopeFactory scopeFactory, IHttpClientFactory httpClientFactory, ILogger<ImageAttachmentHandler> logger)
    {
        _scopeFactory = scopeFactory;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public bool HasImageAttachment(SocketMessage message) =>
        message.Attachments.Any(a => a.ContentType is not null && SupportedMimeTypes.Contains(a.ContentType));

    public async Task HandleAsync(SocketMessage message)
    {
        var attachment = message.Attachments.FirstOrDefault(a =>
            a.ContentType is not null && SupportedMimeTypes.Contains(a.ContentType));

        if (attachment is null) return;

        var prompt = string.IsNullOrWhiteSpace(message.Content)
            ? "Maak een Instagram post draft voor dit product."
            : message.Content;

        _logger.LogInformation("Discord image from {User}: {Filename} ({MimeType})",
            message.Author.Username, attachment.Filename, attachment.ContentType);

        using var typing = message.Channel.EnterTypingState();

        try
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var imageData = await httpClient.GetByteArrayAsync(attachment.Url);

            using var scope = _scopeFactory.CreateScope();
            var ai = scope.ServiceProvider.GetRequiredService<IAiBrainService>();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var draft = await ai.AnalyzeImageAsync(imageData, attachment.ContentType!, prompt);

            var conversation = new ConversationMessage
            {
                Timestamp = DateTime.UtcNow,
                Source = "discord",
                UserId = message.Author.Id.ToString(),
                UserMessage = prompt,
                AiResponse = draft,
                ImageUrl = attachment.Url
            };
            db.ConversationMessages.Add(conversation);
            await db.SaveChangesAsync();

            var embed = EmbedBuilders.BuildImageDraftEmbed(draft, prompt);
            await message.Channel.SendMessageAsync(
                embed: embed,
                messageReference: new global::Discord.MessageReference(message.Id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing image attachment");
            await message.Channel.SendMessageAsync("Er ging iets mis bij het verwerken van de afbeelding. Probeer het later opnieuw.");
        }
    }
}
