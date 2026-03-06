using Discord.WebSocket;
using MarketingAssistant.Core.Interfaces;
using MarketingAssistant.Discord.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MarketingAssistant.Discord.Handlers;

public class ButtonInteractionHandler
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ButtonInteractionHandler> _logger;

    public ButtonInteractionHandler(IServiceScopeFactory scopeFactory, ILogger<ButtonInteractionHandler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task HandleAsync(SocketMessageComponent component)
    {
        var customId = component.Data.CustomId;

        if (customId.StartsWith("action_approve_") || customId.StartsWith("action_reject_"))
        {
            await HandleActionButton(component, customId);
            return;
        }

        await component.RespondAsync("Onbekende interactie.", ephemeral: true);
    }

    private async Task HandleActionButton(SocketMessageComponent component, string customId)
    {
        var isApprove = customId.StartsWith("action_approve_");
        var idPart = customId.Replace("action_approve_", "").Replace("action_reject_", "");

        if (!int.TryParse(idPart, out var actionId))
        {
            await component.RespondAsync("Ongeldige actie ID.", ephemeral: true);
            return;
        }

        await component.DeferAsync();

        using var scope = _scopeFactory.CreateScope();
        var executor = scope.ServiceProvider.GetRequiredService<IActionExecutor>();

        try
        {
            var action = isApprove
                ? await executor.ApproveAsync(actionId, $"discord:{component.User.Username}")
                : await executor.RejectAsync(actionId, $"discord:{component.User.Username}");

            var statusText = isApprove ? "goedgekeurd" : "afgewezen";
            var embed = EmbedBuilders.BuildActionEmbed(action);
            await component.FollowupAsync($"Actie **{statusText}** door {component.User.Username}", embed: embed);

            // Remove buttons from original message
            await component.ModifyOriginalResponseAsync(msg => msg.Components = new global::Discord.ComponentBuilder().Build());
        }
        catch (KeyNotFoundException)
        {
            await component.FollowupAsync("Actie niet gevonden.", ephemeral: true);
        }
        catch (InvalidOperationException ex)
        {
            await component.FollowupAsync($"Fout: {ex.Message}", ephemeral: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling action button {CustomId}", customId);
            await component.FollowupAsync("Er ging iets mis. Probeer het later opnieuw.", ephemeral: true);
        }
    }
}
