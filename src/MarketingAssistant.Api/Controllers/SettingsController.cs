using Microsoft.AspNetCore.Mvc;

namespace MarketingAssistant.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SettingsController : ControllerBase
{
    private readonly IConfiguration _config;

    public SettingsController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet]
    public ActionResult<object> Get()
    {
        return Ok(new
        {
            WooCommerce = new
            {
                BaseUrl = _config["WooCommerce:BaseUrl"] ?? "",
                UseMock = _config.GetValue<bool>("WooCommerce:UseMock"),
                HasCredentials = !string.IsNullOrEmpty(_config["WooCommerce:ConsumerKey"])
            },
            GoogleAnalytics = new
            {
                PropertyId = _config["GoogleAnalytics:PropertyId"] ?? "",
                UseMock = _config.GetValue<bool>("GoogleAnalytics:UseMock"),
                HasCredentials = !string.IsNullOrEmpty(_config["GoogleAnalytics:CredentialsJson"])
            },
            GoogleAds = new
            {
                CustomerAccountId = _config["GoogleAds:CustomerAccountId"] ?? "",
                UseMock = _config.GetValue<bool>("GoogleAds:UseMock"),
                HasCredentials = !string.IsNullOrEmpty(_config["GoogleAds:DeveloperToken"])
            },
            Anthropic = new
            {
                BaseUrl = _config["Anthropic:BaseUrl"] ?? "https://api.anthropic.com",
                Model = _config["Anthropic:Model"] ?? "claude-sonnet-4-20250514",
                HasApiKey = !string.IsNullOrEmpty(_config["Anthropic:ApiKey"])
            },
            Discord = new
            {
                GuildId = _config["Discord:GuildId"] ?? "",
                HasBotToken = !string.IsNullOrEmpty(_config["Discord:BotToken"])
            }
        });
    }

    [HttpPut]
    public ActionResult Update()
    {
        // TODO: Implement settings persistence in Fase 7
        return Ok(new { message = "Settings update not yet implemented" });
    }
}
