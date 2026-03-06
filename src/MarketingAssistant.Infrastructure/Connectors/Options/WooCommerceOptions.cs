namespace MarketingAssistant.Infrastructure.Connectors.Options;

public class WooCommerceOptions
{
    public const string SectionName = "WooCommerce";
    public string BaseUrl { get; set; } = string.Empty;
    public string ConsumerKey { get; set; } = string.Empty;
    public string ConsumerSecret { get; set; } = string.Empty;
    public bool UseMock { get; set; } = true;
}
