namespace MarketingAssistant.Core.Interfaces;

public class AnalyticsData
{
    public int Visitors { get; set; }
    public int Sessions { get; set; }
    public decimal BounceRate { get; set; }
    public decimal ConversionRate { get; set; }
    public Dictionary<string, int> TopPages { get; set; } = new();
    public Dictionary<string, int> TrafficSources { get; set; } = new();
}

public interface IGoogleAnalyticsConnector
{
    Task<AnalyticsData> GetAnalyticsDataAsync(int days = 7, CancellationToken ct = default);
}
