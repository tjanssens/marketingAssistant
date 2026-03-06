namespace MarketingAssistant.Core.DTOs;

public record KpiDto(
    int OrderCount,
    decimal Revenue,
    decimal ConversionRate,
    int Visitors,
    int LowStockCount,
    decimal AdSpend,
    decimal Roas
);
