namespace MarketingAssistant.Core.DTOs;

public record DashboardDto(
    KpiDto Kpis,
    List<AlertDto> RecentAlerts,
    int PendingActionCount,
    BriefingSummaryDto? LatestBriefing
);
