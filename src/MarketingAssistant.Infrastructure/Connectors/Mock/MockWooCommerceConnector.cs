using MarketingAssistant.Core.Interfaces;
using MarketingAssistant.Core.Models;

namespace MarketingAssistant.Infrastructure.Connectors.Mock;

public class MockWooCommerceConnector : IWooCommerceConnector
{
    private static readonly List<Product> Products =
    [
        new() { Id = 1, Name = "Biologische Honing 500g", Sku = "HON-500", Price = 12.95m, StockQuantity = 45, StockStatus = "instock", TotalSales = 234, Category = "Honing" },
        new() { Id = 2, Name = "Lavendel Honing 250g", Sku = "HON-LAV-250", Price = 8.50m, StockQuantity = 3, StockStatus = "instock", TotalSales = 189, Category = "Honing" },
        new() { Id = 3, Name = "Manuka Honing MGO 400+", Sku = "HON-MAN-400", Price = 34.95m, StockQuantity = 12, StockStatus = "instock", TotalSales = 87, Category = "Honing" },
        new() { Id = 4, Name = "Bijenwas Kaarsen Set (3st)", Sku = "KAARS-SET3", Price = 19.95m, StockQuantity = 28, StockStatus = "instock", TotalSales = 156, Category = "Kaarsen" },
        new() { Id = 5, Name = "Propolis Tinctuur 30ml", Sku = "PROP-30", Price = 15.50m, StockQuantity = 2, StockStatus = "instock", TotalSales = 98, Category = "Supplementen" },
        new() { Id = 6, Name = "Honingraat Rauw 400g", Sku = "RAAT-400", Price = 22.00m, StockQuantity = 0, StockStatus = "outofstock", TotalSales = 67, Category = "Honing" },
        new() { Id = 7, Name = "Lippenbalsem Bijenwas", Sku = "LIP-BWAS", Price = 4.95m, StockQuantity = 85, StockStatus = "instock", TotalSales = 312, Category = "Verzorging" },
        new() { Id = 8, Name = "Cadeaupakket De Imker", Sku = "CADEAU-IMK", Price = 44.95m, StockQuantity = 15, StockStatus = "instock", TotalSales = 43, Category = "Cadeaus" },
        new() { Id = 9, Name = "Stuifmeel Granulaat 200g", Sku = "STUIF-200", Price = 11.95m, StockQuantity = 4, StockStatus = "instock", TotalSales = 76, Category = "Supplementen" },
        new() { Id = 10, Name = "Honing Shampoo Bar", Sku = "SHAM-HON", Price = 9.50m, StockQuantity = 32, StockStatus = "instock", TotalSales = 145, Category = "Verzorging" },
    ];

    public Task<IReadOnlyList<Order>> GetRecentOrdersAsync(int days = 7, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var orders = new List<Order>();

        for (var i = 0; i < 23; i++)
        {
            var orderDate = now.AddDays(-Random.Shared.Next(0, days)).AddHours(-Random.Shared.Next(0, 24));
            var itemCount = Random.Shared.Next(1, 4);
            var items = Enumerable.Range(0, itemCount)
                .Select(_ =>
                {
                    var product = Products[Random.Shared.Next(Products.Count)];
                    var qty = Random.Shared.Next(1, 3);
                    return new OrderItem
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = qty,
                        Price = product.Price
                    };
                })
                .ToList();

            orders.Add(new Order
            {
                Id = 1000 + i,
                OrderNumber = $"#{1000 + i}",
                CreatedAt = orderDate,
                Total = items.Sum(x => x.Price * x.Quantity),
                Status = i < 20 ? "completed" : "processing",
                CustomerEmail = $"klant{i + 1}@voorbeeld.nl",
                Items = items
            });
        }

        return Task.FromResult<IReadOnlyList<Order>>(orders);
    }

    public Task<IReadOnlyList<Product>> GetProductsAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyList<Product>>(Products);
    }

    public Task<IReadOnlyList<Product>> GetLowStockProductsAsync(int threshold = 5, CancellationToken ct = default)
    {
        var lowStock = Products.Where(p => p.StockQuantity <= threshold).ToList();
        return Task.FromResult<IReadOnlyList<Product>>(lowStock);
    }
}
