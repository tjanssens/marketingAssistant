using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MarketingAssistant.Core.Interfaces;
using MarketingAssistant.Core.Models;
using MarketingAssistant.Infrastructure.Connectors.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MarketingAssistant.Infrastructure.Connectors;

public class WooCommerceConnector : IWooCommerceConnector
{
    private readonly HttpClient _http;
    private readonly WooCommerceOptions _options;
    private readonly ILogger<WooCommerceConnector> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public WooCommerceConnector(HttpClient http, IOptions<WooCommerceOptions> options, ILogger<WooCommerceConnector> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;

        _http.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/wp-json/wc/v3/");
    }

    public async Task<IReadOnlyList<Order>> GetRecentOrdersAsync(int days = 7, CancellationToken ct = default)
    {
        var after = DateTime.UtcNow.AddDays(-days).ToString("o");
        var url = $"orders?after={after}&per_page=100&consumer_key={_options.ConsumerKey}&consumer_secret={_options.ConsumerSecret}";

        var wcOrders = await _http.GetFromJsonAsync<List<WcOrder>>(url, JsonOptions, ct) ?? [];

        _logger.LogInformation("WooCommerce: fetched {Count} orders", wcOrders.Count);

        return wcOrders.Select(o => new Order
        {
            Id = o.Id,
            OrderNumber = o.Number ?? o.Id.ToString(),
            CreatedAt = o.DateCreated,
            Total = decimal.TryParse(o.Total, out var t) ? t : 0,
            Status = o.Status ?? "unknown",
            CustomerEmail = o.Billing?.Email ?? "",
            Items = o.LineItems?.Select(li => new OrderItem
            {
                ProductId = li.ProductId,
                ProductName = li.Name ?? "",
                Quantity = li.Quantity,
                Price = decimal.TryParse(li.Price, out var p) ? p : 0
            }).ToList() ?? []
        }).ToList();
    }

    public async Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken ct = default)
    {
        var url = $"products?per_page=100&consumer_key={_options.ConsumerKey}&consumer_secret={_options.ConsumerSecret}";
        var wcProducts = await _http.GetFromJsonAsync<List<WcProduct>>(url, JsonOptions, ct) ?? [];

        _logger.LogInformation("WooCommerce: fetched {Count} products", wcProducts.Count);

        return wcProducts.Select(MapProduct).ToList();
    }

    public async Task<IReadOnlyList<Product>> GetLowStockProductsAsync(int threshold = 5, CancellationToken ct = default)
    {
        var url = $"products?stock_status=instock&per_page=100&consumer_key={_options.ConsumerKey}&consumer_secret={_options.ConsumerSecret}";
        var wcProducts = await _http.GetFromJsonAsync<List<WcProduct>>(url, JsonOptions, ct) ?? [];

        var lowStock = wcProducts
            .Where(p => p.ManageStock && p.StockQuantity.HasValue && p.StockQuantity.Value <= threshold)
            .Select(MapProduct)
            .ToList();

        _logger.LogInformation("WooCommerce: {Count} low stock products (threshold: {Threshold})", lowStock.Count, threshold);
        return lowStock;
    }

    private static Product MapProduct(WcProduct p) => new()
    {
        Id = p.Id,
        Name = p.Name ?? "",
        Sku = p.Sku ?? "",
        Price = decimal.TryParse(p.Price, out var pr) ? pr : 0,
        StockQuantity = p.StockQuantity ?? 0,
        StockStatus = p.StockStatus ?? "unknown",
        TotalSales = p.TotalSales,
        Category = p.Categories?.FirstOrDefault()?.Name ?? ""
    };

    // WooCommerce REST API v3 response models
    private record WcOrder
    {
        public int Id { get; init; }
        public string? Number { get; init; }
        [JsonPropertyName("date_created")]
        public DateTime DateCreated { get; init; }
        public string? Total { get; init; }
        public string? Status { get; init; }
        public WcBilling? Billing { get; init; }
        [JsonPropertyName("line_items")]
        public List<WcLineItem>? LineItems { get; init; }
    }

    private record WcBilling { public string? Email { get; init; } }

    private record WcLineItem
    {
        [JsonPropertyName("product_id")]
        public int ProductId { get; init; }
        public string? Name { get; init; }
        public int Quantity { get; init; }
        public string? Price { get; init; }
    }

    private record WcProduct
    {
        public int Id { get; init; }
        public string? Name { get; init; }
        public string? Sku { get; init; }
        public string? Price { get; init; }
        [JsonPropertyName("stock_quantity")]
        public int? StockQuantity { get; init; }
        [JsonPropertyName("stock_status")]
        public string? StockStatus { get; init; }
        [JsonPropertyName("manage_stock")]
        public bool ManageStock { get; init; }
        [JsonPropertyName("total_sales")]
        public int TotalSales { get; init; }
        public List<WcCategory>? Categories { get; init; }
    }

    private record WcCategory { public string? Name { get; init; } }
}
