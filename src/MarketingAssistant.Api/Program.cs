using Discord;
using Discord.WebSocket;
using MarketingAssistant.Api.Hubs;
using MarketingAssistant.Api.Middleware;
using MarketingAssistant.Core.Interfaces;
using MarketingAssistant.Discord;
using MarketingAssistant.Discord.Handlers;
using MarketingAssistant.Infrastructure.AI;
using MarketingAssistant.Infrastructure.Connectors;
using MarketingAssistant.Infrastructure.Connectors.Mock;
using MarketingAssistant.Infrastructure.Connectors.Options;
using MarketingAssistant.Infrastructure.Data;
using MarketingAssistant.Infrastructure.Services;
using MarketingAssistant.Scheduling;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=marketingassistant.db"));

// Controllers + SignalR
builder.Services.AddControllers();
builder.Services.AddSignalR();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS for Angular dev
builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Connectors (mock/real toggle)
if (builder.Configuration.GetValue<bool>("DevMode:UseMockConnectors"))
{
    builder.Services.AddScoped<IWooCommerceConnector, MockWooCommerceConnector>();
    builder.Services.AddScoped<IGoogleAnalyticsConnector, MockGoogleAnalyticsConnector>();
    builder.Services.AddScoped<IGoogleAdsConnector, MockGoogleAdsConnector>();
}
else
{
    builder.Services.Configure<WooCommerceOptions>(builder.Configuration.GetSection(WooCommerceOptions.SectionName));
    builder.Services.Configure<GoogleAnalyticsOptions>(builder.Configuration.GetSection(GoogleAnalyticsOptions.SectionName));
    builder.Services.Configure<GoogleAdsOptions>(builder.Configuration.GetSection(GoogleAdsOptions.SectionName));

    builder.Services.AddHttpClient<IWooCommerceConnector, WooCommerceConnector>();
    builder.Services.AddScoped<IGoogleAnalyticsConnector, GoogleAnalyticsConnector>();
    builder.Services.AddScoped<IGoogleAdsConnector, GoogleAdsConnector>();
}

// AI Brain
builder.Services.Configure<AnthropicOptions>(builder.Configuration.GetSection(AnthropicOptions.SectionName));
builder.Services.AddHttpClient<IAiBrainService, ClaudeAiBrainService>((sp, client) =>
{
    var options = builder.Configuration.GetSection(AnthropicOptions.SectionName).Get<AnthropicOptions>() ?? new AnthropicOptions();
    client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
    client.DefaultRequestHeaders.Add("x-api-key", options.ApiKey);
    client.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
});

// Services
builder.Services.AddScoped<DataAggregator>();
builder.Services.AddScoped<BriefingService>();
builder.Services.AddScoped<IActionExecutor, ActionExecutor>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<AlertService>();

// Discord bot
builder.Services.Configure<DiscordOptions>(builder.Configuration.GetSection(DiscordOptions.SectionName));
var discordToken = builder.Configuration["Discord:BotToken"];
if (!string.IsNullOrEmpty(discordToken))
{
    builder.Services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
    {
        GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent
    }));
    builder.Services.AddSingleton<MessageHandler>();
    builder.Services.AddSingleton<ButtonInteractionHandler>();
    builder.Services.AddSingleton<ImageAttachmentHandler>();
    builder.Services.AddSingleton<IDiscordNotifier, DiscordNotifier>();
    builder.Services.AddHostedService<DiscordBotService>();
}

// Background jobs
builder.Services.AddHostedService<DailyBriefingJob>();
builder.Services.AddHostedService<HourlyAlertCheckJob>();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Angular");
app.MapControllers();
app.MapHub<DashboardHub>("/hubs/dashboard");

// Health check
app.MapGet("/api/health", () => Results.Ok(new
{
    Status = "Healthy",
    Timestamp = DateTime.UtcNow,
    Version = "1.0.0-mvp"
}));

app.Run();
