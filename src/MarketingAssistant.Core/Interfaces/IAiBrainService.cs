using MarketingAssistant.Core.Models;

namespace MarketingAssistant.Core.Interfaces;

public class AggregatedData
{
    public IReadOnlyList<Order> RecentOrders { get; set; } = [];
    public IReadOnlyList<Product> Products { get; set; } = [];
    public IReadOnlyList<Product> LowStockProducts { get; set; } = [];
    public AnalyticsData Analytics { get; set; } = new();
    public AdsData Ads { get; set; } = new();
    public KpiSnapshot? PreviousKpi { get; set; }
}

public interface IAiBrainService
{
    Task<Briefing> GenerateBriefingAsync(AggregatedData data, CancellationToken ct = default);
    Task<string> AnswerQuestionAsync(string question, AggregatedData context, CancellationToken ct = default);
    Task<string> AnalyzeImageAsync(byte[] imageData, string mimeType, string prompt, CancellationToken ct = default);
    Task<IReadOnlyList<ActionItem>> SuggestActionsAsync(AggregatedData data, CancellationToken ct = default);
}
