namespace MarketingAssistant.Core.Models;

public class KpiSnapshot
{
    public int Id { get; set; }
    public DateTime CapturedAt { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal ConversionRate { get; set; }
    public int Visitors { get; set; }
    public int LowStockCount { get; set; }
    public decimal AdSpend { get; set; }
    public decimal Roas { get; set; }
    public string RawData { get; set; } = "{}";
}
