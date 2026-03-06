namespace MarketingAssistant.Infrastructure.Connectors.Options;

public class GoogleAdsOptions
{
    public const string SectionName = "GoogleAds";
    public string CustomerAccountId { get; set; } = string.Empty;
    public string DeveloperToken { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public bool UseMock { get; set; } = true;
}
