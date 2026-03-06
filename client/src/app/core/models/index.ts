export interface KpiDto {
  orderCount: number;
  revenue: number;
  conversionRate: number;
  visitors: number;
  lowStockCount: number;
  adSpend: number;
  roas: number;
}

export interface AlertDto {
  id: number;
  createdAt: string;
  severity: 'Info' | 'Warning' | 'Critical';
  title: string;
  message: string;
  category: string;
  isAcknowledged: boolean;
}

export interface BriefingSummaryDto {
  id: number;
  generatedAt: string;
  title: string;
  period: string;
  actionCount: number;
}

export interface BriefingDto {
  id: number;
  generatedAt: string;
  title: string;
  content: string;
  period: string;
  actions: ActionItemDto[];
}

export interface ActionItemDto {
  id: number;
  briefingId: number | null;
  description: string;
  type: 'BudgetShift' | 'PauseCampaign' | 'StockAlert' | 'PriceAdjust' | 'ContentPost';
  status: 'Pending' | 'Approved' | 'Rejected' | 'Executed' | 'Failed';
  suggestedAt: string;
  resolvedAt: string | null;
  resolvedBy: string | null;
  aiReasoning: string;
}

export interface DashboardDto {
  kpis: KpiDto;
  recentAlerts: AlertDto[];
  pendingActionCount: number;
  latestBriefing: BriefingSummaryDto | null;
}

export interface HealthResponse {
  status: string;
  timestamp: string;
  version: string;
}
