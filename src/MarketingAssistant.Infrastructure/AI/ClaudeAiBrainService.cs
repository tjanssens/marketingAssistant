using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using MarketingAssistant.Core.Enums;
using MarketingAssistant.Core.Interfaces;
using MarketingAssistant.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MarketingAssistant.Infrastructure.AI;

public partial class ClaudeAiBrainService : IAiBrainService
{
    private readonly HttpClient _http;
    private readonly AnthropicOptions _options;
    private readonly ILogger<ClaudeAiBrainService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public ClaudeAiBrainService(HttpClient http, IOptions<AnthropicOptions> options, ILogger<ClaudeAiBrainService> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Briefing> GenerateBriefingAsync(AggregatedData data, CancellationToken ct = default)
    {
        var prompt = LoadPrompt("BriefingPrompt.txt")
            .Replace("{{orders}}", SummarizeOrders(data.RecentOrders))
            .Replace("{{lowStock}}", SummarizeProducts(data.LowStockProducts))
            .Replace("{{analytics}}", SummarizeAnalytics(data.Analytics))
            .Replace("{{ads}}", SummarizeAds(data.Ads))
            .Replace("{{previousKpi}}", data.PreviousKpi is not null
                ? JsonSerializer.Serialize(data.PreviousKpi, JsonOptions)
                : "Geen vorige data beschikbaar");

        var response = await SendMessageAsync(prompt, ct);

        var briefing = new Briefing
        {
            GeneratedAt = DateTime.UtcNow,
            Title = $"Dagelijkse Briefing - {DateTime.UtcNow.ToString("dd MMMM yyyy", new CultureInfo("nl-NL"))}",
            Content = response,
            RawData = JsonSerializer.Serialize(data, JsonOptions),
            Period = "7 dagen",
            Actions = ParseActions(response)
        };

        return briefing;
    }

    public async Task<string> AnswerQuestionAsync(string question, AggregatedData context, CancellationToken ct = default)
    {
        var prompt = LoadPrompt("QuestionPrompt.txt")
            .Replace("{{orders}}", SummarizeOrders(context.RecentOrders))
            .Replace("{{products}}", SummarizeProducts(context.Products))
            .Replace("{{analytics}}", SummarizeAnalytics(context.Analytics))
            .Replace("{{ads}}", SummarizeAds(context.Ads))
            .Replace("{{question}}", question);

        return await SendMessageAsync(prompt, ct);
    }

    public async Task<string> AnalyzeImageAsync(byte[] imageData, string mimeType, string prompt, CancellationToken ct = default)
    {
        var imagePrompt = LoadPrompt("ImagePrompt.txt")
            .Replace("{{prompt}}", prompt);

        var base64 = Convert.ToBase64String(imageData);

        var request = new
        {
            model = _options.Model,
            max_tokens = _options.MaxTokens,
            messages = new object[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "image",
                            source = new
                            {
                                type = "base64",
                                media_type = mimeType,
                                data = base64
                            }
                        },
                        new
                        {
                            type = "text",
                            text = imagePrompt
                        }
                    }
                }
            }
        };

        return await SendRequestAsync(request, ct);
    }

    public async Task<IReadOnlyList<ActionItem>> SuggestActionsAsync(AggregatedData data, CancellationToken ct = default)
    {
        var briefing = await GenerateBriefingAsync(data, ct);
        return briefing.Actions;
    }

    private async Task<string> SendMessageAsync(string userMessage, CancellationToken ct)
    {
        var request = new
        {
            model = _options.Model,
            max_tokens = _options.MaxTokens,
            messages = new[]
            {
                new { role = "user", content = userMessage }
            }
        };

        return await SendRequestAsync(request, ct);
    }

    private async Task<string> SendRequestAsync(object request, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(request, JsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await _http.PostAsync("v1/messages", content, ct);

        var responseBody = await response.Content.ReadAsStringAsync(ct);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Anthropic API error {StatusCode}: {Body}", response.StatusCode, responseBody);
            throw new HttpRequestException($"Anthropic API returned {response.StatusCode}: {responseBody}");
        }

        using var doc = JsonDocument.Parse(responseBody);
        var contentArray = doc.RootElement.GetProperty("content");
        var sb = new StringBuilder();

        foreach (var block in contentArray.EnumerateArray())
        {
            if (block.GetProperty("type").GetString() == "text")
            {
                sb.Append(block.GetProperty("text").GetString());
            }
        }

        return sb.ToString();
    }

    private static List<ActionItem> ParseActions(string content)
    {
        var actions = new List<ActionItem>();
        var matches = ActionRegex().Matches(content);

        foreach (Match match in matches)
        {
            var typeStr = match.Groups[1].Value;
            var description = match.Groups[2].Value.Trim();

            if (Enum.TryParse<ActionType>(typeStr, true, out var actionType))
            {
                actions.Add(new ActionItem
                {
                    Description = description,
                    Type = actionType,
                    Status = ActionStatus.Pending,
                    SuggestedAt = DateTime.UtcNow,
                    AiReasoning = description
                });
            }
        }

        return actions;
    }

    private static string LoadPrompt(string filename)
    {
        var assembly = typeof(ClaudeAiBrainService).Assembly;
        var resourceName = $"MarketingAssistant.Infrastructure.AI.Prompts.{filename}";
        using var stream = assembly.GetManifestResourceStream(resourceName);

        if (stream is null)
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "AI", "Prompts");
            var filePath = Path.Combine(dir, filename);
            if (File.Exists(filePath))
                return File.ReadAllText(filePath);

            throw new FileNotFoundException($"Prompt file not found: {filename}");
        }

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static string SummarizeOrders(IReadOnlyList<Order> orders)
    {
        if (orders.Count == 0) return "Geen bestellingen";
        var total = orders.Sum(o => o.Total);
        var sb = new StringBuilder();
        sb.AppendLine($"Totaal: {orders.Count} bestellingen, €{total:F2} omzet");
        foreach (var order in orders.Take(10))
        {
            sb.AppendLine($"- {order.OrderNumber}: €{order.Total:F2} ({order.Status}) - {order.Items.Count} items");
        }
        if (orders.Count > 10) sb.AppendLine($"... en {orders.Count - 10} meer");
        return sb.ToString();
    }

    private static string SummarizeProducts(IReadOnlyList<Product> products)
    {
        if (products.Count == 0) return "Geen producten";
        var sb = new StringBuilder();
        foreach (var p in products)
        {
            sb.AppendLine($"- {p.Name} (SKU: {p.Sku}): €{p.Price:F2}, voorraad: {p.StockQuantity}, verkopen: {p.TotalSales}");
        }
        return sb.ToString();
    }

    private static string SummarizeAnalytics(AnalyticsData data)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Bezoekers: {data.Visitors}, Sessies: {data.Sessions}");
        sb.AppendLine($"Bouncepercentage: {data.BounceRate}%, Conversie: {data.ConversionRate}%");
        sb.AppendLine("Top pagina's:");
        foreach (var (page, views) in data.TopPages.Take(5))
        {
            sb.AppendLine($"  {page}: {views} weergaven");
        }
        sb.AppendLine("Verkeersbronnen:");
        foreach (var (source, count) in data.TrafficSources)
        {
            sb.AppendLine($"  {source}: {count}");
        }
        return sb.ToString();
    }

    private static string SummarizeAds(AdsData data)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Totale uitgaven: €{data.TotalSpend:F2}, Omzet: €{data.TotalRevenue:F2}, ROAS: {data.Roas:F2}x");
        sb.AppendLine($"Clicks: {data.Clicks}, Impressies: {data.Impressions}, CTR: {data.Ctr:F2}%");
        sb.AppendLine("Campagnes:");
        foreach (var c in data.Campaigns)
        {
            sb.AppendLine($"  {c.Name} [{c.Status}]: €{c.Spend:F2}/€{c.Budget:F2}, ROAS: {c.Roas:F2}x, {c.Conversions} conversies");
        }
        return sb.ToString();
    }

    [GeneratedRegex(@"\[ACTIE type=""(\w+)""\]\s*(.*?)\s*\[/ACTIE\]", RegexOptions.Singleline)]
    private static partial Regex ActionRegex();
}
