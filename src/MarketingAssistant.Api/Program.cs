using MarketingAssistant.Api.Hubs;
using MarketingAssistant.Api.Middleware;
using MarketingAssistant.Core.Interfaces;
using MarketingAssistant.Infrastructure.Connectors.Mock;
using MarketingAssistant.Infrastructure.Data;
using MarketingAssistant.Infrastructure.Services;
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
    builder.Services.AddSingleton<IWooCommerceConnector, MockWooCommerceConnector>();
    builder.Services.AddSingleton<IGoogleAnalyticsConnector, MockGoogleAnalyticsConnector>();
    builder.Services.AddSingleton<IGoogleAdsConnector, MockGoogleAdsConnector>();
}

// Services
builder.Services.AddScoped<DataAggregator>();

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
