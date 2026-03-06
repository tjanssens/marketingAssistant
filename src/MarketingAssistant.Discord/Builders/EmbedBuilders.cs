using Discord;
using MarketingAssistant.Core.Enums;
using MarketingAssistant.Core.Models;

namespace MarketingAssistant.Discord.Builders;

public static class EmbedBuilders
{
    public static Embed BuildBriefingEmbed(Briefing briefing)
    {
        var builder = new EmbedBuilder()
            .WithTitle(briefing.Title)
            .WithDescription(Truncate(briefing.Content, 4000))
            .WithColor(Color.Blue)
            .WithTimestamp(briefing.GeneratedAt)
            .WithFooter($"Periode: {briefing.Period}");

        if (briefing.Actions.Count > 0)
        {
            builder.AddField("Voorgestelde acties", $"{briefing.Actions.Count} actie(s) wachten op goedkeuring");
        }

        return builder.Build();
    }

    public static Embed BuildAlertEmbed(Alert alert)
    {
        var color = alert.Severity switch
        {
            AlertSeverity.Critical => Color.Red,
            AlertSeverity.Warning => Color.Orange,
            _ => Color.Blue
        };

        return new EmbedBuilder()
            .WithTitle($"{SeverityEmoji(alert.Severity)} {alert.Title}")
            .WithDescription(alert.Message)
            .WithColor(color)
            .WithTimestamp(alert.CreatedAt)
            .AddField("Categorie", alert.Category, true)
            .AddField("Ernst", alert.Severity.ToString(), true)
            .Build();
    }

    public static Embed BuildActionEmbed(ActionItem action)
    {
        var color = action.Status switch
        {
            ActionStatus.Approved => Color.Green,
            ActionStatus.Rejected => Color.Red,
            ActionStatus.Executed => Color.DarkGreen,
            ActionStatus.Failed => Color.DarkRed,
            _ => Color.Gold
        };

        var builder = new EmbedBuilder()
            .WithTitle(action.Description)
            .WithDescription(action.AiReasoning)
            .WithColor(color)
            .AddField("Type", action.Type.ToString(), true)
            .AddField("Status", action.Status.ToString(), true);

        if (action.ResolvedBy is not null)
            builder.AddField("Afgehandeld door", action.ResolvedBy, true);

        return builder.Build();
    }

    public static Embed BuildAnswerEmbed(string question, string answer)
    {
        return new EmbedBuilder()
            .WithTitle("Antwoord")
            .WithDescription(Truncate(answer, 4000))
            .WithColor(Color.Teal)
            .WithFooter($"Vraag: {Truncate(question, 200)}")
            .Build();
    }

    public static Embed BuildImageDraftEmbed(string draft, string prompt)
    {
        return new EmbedBuilder()
            .WithTitle("Instagram Post Draft")
            .WithDescription(Truncate(draft, 4000))
            .WithColor(Color.Purple)
            .WithFooter($"Prompt: {Truncate(prompt, 200)}")
            .Build();
    }

    public static ComponentBuilder BuildActionButtons(int actionId)
    {
        return new ComponentBuilder()
            .WithButton("Goedkeuren", $"action_approve_{actionId}", ButtonStyle.Success)
            .WithButton("Afwijzen", $"action_reject_{actionId}", ButtonStyle.Danger);
    }

    private static string SeverityEmoji(AlertSeverity severity) => severity switch
    {
        AlertSeverity.Critical => "\u26a0\ufe0f",
        AlertSeverity.Warning => "\u26a1",
        _ => "\u2139\ufe0f"
    };

    private static string Truncate(string text, int maxLength) =>
        text.Length <= maxLength ? text : text[..(maxLength - 3)] + "...";
}
