import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  DashboardDto,
  BriefingSummaryDto,
  BriefingDto,
  ActionItemDto,
  AlertDto,
  HealthResponse,
} from '../models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api';

  getHealth(): Observable<HealthResponse> {
    return this.http.get<HealthResponse>(`${this.baseUrl}/health`);
  }

  getDashboard(): Observable<DashboardDto> {
    return this.http.get<DashboardDto>(`${this.baseUrl}/dashboard`);
  }

  getBriefings(): Observable<BriefingSummaryDto[]> {
    return this.http.get<BriefingSummaryDto[]>(`${this.baseUrl}/briefings`);
  }

  getBriefing(id: number): Observable<BriefingDto> {
    return this.http.get<BriefingDto>(`${this.baseUrl}/briefings/${id}`);
  }

  generateBriefing(): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/briefings/generate`, {});
  }

  getActions(status?: string): Observable<ActionItemDto[]> {
    const url = status
      ? `${this.baseUrl}/actions?status=${status}`
      : `${this.baseUrl}/actions`;
    return this.http.get<ActionItemDto[]>(url);
  }

  approveAction(id: number): Observable<ActionItemDto> {
    return this.http.post<ActionItemDto>(`${this.baseUrl}/actions/${id}/approve`, {});
  }

  rejectAction(id: number): Observable<ActionItemDto> {
    return this.http.post<ActionItemDto>(`${this.baseUrl}/actions/${id}/reject`, {});
  }

  getAlerts(): Observable<AlertDto[]> {
    return this.http.get<AlertDto[]>(`${this.baseUrl}/alerts`);
  }

  getSettings(): Observable<unknown> {
    return this.http.get(`${this.baseUrl}/settings`);
  }
}
