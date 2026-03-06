using Discord;
using Discord.WebSocket;
using MarketingAssistant.Discord.Handlers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MarketingAssistant.Discord;

public class DiscordBotService : IHostedService
{
    private readonly DiscordSocketClient _client;
    private readonly DiscordOptions _options;
    private readonly MessageHandler _messageHandler;
    private readonly ButtonInteractionHandler _buttonHandler;
    private readonly ImageAttachmentHandler _imageHandler;
    private readonly ILogger<DiscordBotService> _logger;

    public DiscordBotService(
        DiscordSocketClient client,
        IOptions<DiscordOptions> options,
        MessageHandler messageHandler,
        ButtonInteractionHandler buttonHandler,
        ImageAttachmentHandler imageHandler,
        ILogger<DiscordBotService> logger)
    {
        _client = client;
        _options = options.Value;
        _messageHandler = messageHandler;
        _buttonHandler = buttonHandler;
        _imageHandler = imageHandler;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_options.BotToken))
        {
            _logger.LogWarning("Discord bot token not configured — bot will not start");
            return;
        }

        _client.Log += LogDiscordMessage;
        _client.Ready += OnReady;
        _client.MessageReceived += OnMessageReceived;
        _client.ButtonExecuted += OnButtonExecuted;

        await _client.LoginAsync(TokenType.Bot, _options.BotToken);
        await _client.StartAsync();

        _logger.LogInformation("Discord bot starting...");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_client.LoginState == LoginState.LoggedIn)
        {
            await _client.StopAsync();
            _logger.LogInformation("Discord bot stopped");
        }
    }

    private Task OnReady()
    {
        _logger.LogInformation("Discord bot connected as {BotUser}", _client.CurrentUser?.Username);
        return Task.CompletedTask;
    }

    private async Task OnMessageReceived(SocketMessage message)
    {
        if (message.Author.IsBot) return;

        if (_imageHandler.HasImageAttachment(message))
        {
            await _imageHandler.HandleAsync(message);
        }
        else
        {
            await _messageHandler.HandleAsync(message);
        }
    }

    private async Task OnButtonExecuted(SocketMessageComponent component)
    {
        await _buttonHandler.HandleAsync(component);
    }

    private Task LogDiscordMessage(LogMessage msg)
    {
        var level = msg.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Debug,
            LogSeverity.Debug => LogLevel.Trace,
            _ => LogLevel.Information
        };

        _logger.Log(level, msg.Exception, "Discord: {Message}", msg.Message);
        return Task.CompletedTask;
    }
}
