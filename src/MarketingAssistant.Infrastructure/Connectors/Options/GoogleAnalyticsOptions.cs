namespace MarketingAssistant.Infrastructure.Connectors.Options;

public class GoogleAnalyticsOptions
{
    public const string SectionName = "GoogleAnalytics";
    public string PropertyId { get; set; } = string.Empty;
    public string CredentialsJson { get; set; } = string.Empty;
    public bool UseMock { get; set; } = true;
}
