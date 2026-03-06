import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import { SignalRService } from '../../core/services/signalr.service';
import { DashboardDto } from '../../core/models';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule, MatChipsModule, RouterLink],
  template: `
    <div class="dashboard-grid">
      @if (data) {
        <mat-card>
          <mat-card-header><mat-card-title>Omzet</mat-card-title></mat-card-header>
          <mat-card-content><span class="kpi-value">&euro;{{ data.kpis.revenue | number:'1.2-2' }}</span></mat-card-content>
        </mat-card>
        <mat-card>
          <mat-card-header><mat-card-title>Bestellingen</mat-card-title></mat-card-header>
          <mat-card-content><span class="kpi-value">{{ data.kpis.orderCount }}</span></mat-card-content>
        </mat-card>
        <mat-card>
          <mat-card-header><mat-card-title>Bezoekers</mat-card-title></mat-card-header>
          <mat-card-content><span class="kpi-value">{{ data.kpis.visitors | number }}</span></mat-card-content>
        </mat-card>
        <mat-card>
          <mat-card-header><mat-card-title>Conversie</mat-card-title></mat-card-header>
          <mat-card-content><span class="kpi-value">{{ data.kpis.conversionRate }}%</span></mat-card-content>
        </mat-card>
        <mat-card>
          <mat-card-header><mat-card-title>Ad Spend</mat-card-title></mat-card-header>
          <mat-card-content><span class="kpi-value">&euro;{{ data.kpis.adSpend | number:'1.2-2' }}</span></mat-card-content>
        </mat-card>
        <mat-card>
          <mat-card-header><mat-card-title>ROAS</mat-card-title></mat-card-header>
          <mat-card-content><span class="kpi-value">{{ data.kpis.roas | number:'1.2-2' }}x</span></mat-card-content>
        </mat-card>
        <mat-card>
          <mat-card-header><mat-card-title>Lage Voorraad</mat-card-title></mat-card-header>
          <mat-card-content><span class="kpi-value">{{ data.kpis.lowStockCount }}</span></mat-card-content>
        </mat-card>
        <mat-card>
          <mat-card-header><mat-card-title>Openstaande Acties</mat-card-title></mat-card-header>
          <mat-card-content>
            <span class="kpi-value">{{ data.pendingActionCount }}</span>
            @if (data.pendingActionCount > 0) {
              <a routerLink="/actions">Bekijk</a>
            }
          </mat-card-content>
        </mat-card>

        @if (data.recentAlerts.length > 0) {
          <mat-card class="alerts-card">
            <mat-card-header><mat-card-title>Recente Alerts</mat-card-title></mat-card-header>
            <mat-card-content>
              @for (alert of data.recentAlerts; track alert.id) {
                <div class="alert-item" [class]="'severity-' + alert.severity.toLowerCase()">
                  <mat-chip>{{ alert.severity }}</mat-chip>
                  <strong>{{ alert.title }}</strong>
                  <span>{{ alert.message }}</span>
                </div>
              }
            </mat-card-content>
          </mat-card>
        }

        @if (data.latestBriefing) {
          <mat-card class="briefing-card">
            <mat-card-header><mat-card-title>Laatste Briefing</mat-card-title></mat-card-header>
            <mat-card-content>
              <a [routerLink]="['/briefings', data.latestBriefing.id]">
                {{ data.latestBriefing.title }}
              </a>
              <span class="briefing-date">{{ data.latestBriefing.generatedAt | date:'short' }}</span>
            </mat-card-content>
          </mat-card>
        }
      } @else {
        <p>Dashboard laden...</p>
      }
    </div>
  `,
  styles: [`
    .dashboard-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
      gap: 16px;
      padding: 16px;
    }
    .kpi-value { font-size: 2rem; font-weight: bold; }
    .alerts-card, .briefing-card { grid-column: 1 / -1; }
    .alert-item { display: flex; align-items: center; gap: 8px; padding: 8px 0; }
    .severity-critical { border-left: 4px solid #f44336; padding-left: 8px; }
    .severity-warning { border-left: 4px solid #ff9800; padding-left: 8px; }
    .severity-info { border-left: 4px solid #2196f3; padding-left: 8px; }
  `]
})
export class DashboardComponent implements OnInit, OnDestroy {
  private readonly api = inject(ApiService);
  private readonly signalR = inject(SignalRService);
  private subscriptions: Subscription[] = [];
  data: DashboardDto | null = null;

  ngOnInit(): void {
    this.loadDashboard();
    this.subscriptions.push(
      this.signalR.newBriefing$.subscribe(() => this.loadDashboard()),
      this.signalR.newAlert$.subscribe(() => this.loadDashboard()),
      this.signalR.actionUpdated$.subscribe(() => this.loadDashboard()),
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(s => s.unsubscribe());
  }

  private loadDashboard(): void {
    this.api.getDashboard().subscribe(d => this.data = d);
  }
}
