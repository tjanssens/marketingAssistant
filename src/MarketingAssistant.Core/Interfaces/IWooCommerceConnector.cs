using MarketingAssistant.Core.Models;

namespace MarketingAssistant.Core.Interfaces;

public interface IWooCommerceConnector
{
    Task<IReadOnlyList<Order>> GetRecentOrdersAsync(int days = 7, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetLowStockProductsAsync(int threshold = 5, CancellationToken ct = default);
}
