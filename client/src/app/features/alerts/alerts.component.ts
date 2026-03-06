import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatIconModule } from '@angular/material/icon';
import { Subscription } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import { SignalRService } from '../../core/services/signalr.service';
import { AlertDto } from '../../core/models';

@Component({
  selector: 'app-alerts',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatChipsModule, MatIconModule],
  template: `
    <div class="alerts-container">
      <h2>Alerts</h2>

      @for (alert of alerts; track alert.id) {
        <mat-card class="alert-card" [class]="'severity-' + alert.severity.toLowerCase()">
          <mat-card-header>
            <mat-card-title>
              <span class="severity-icon">{{ severityIcon(alert.severity) }}</span>
              {{ alert.title }}
            </mat-card-title>
            <mat-card-subtitle>{{ alert.createdAt | date:'medium' }}</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <p>{{ alert.message }}</p>
            <div class="chips">
              <mat-chip>{{ alert.category }}</mat-chip>
              <mat-chip [class]="'chip-' + alert.severity.toLowerCase()">{{ alert.severity }}</mat-chip>
            </div>
          </mat-card-content>
        </mat-card>
      } @empty {
        <p>Geen alerts gevonden.</p>
      }
    </div>
  `,
  styles: [`
    .alerts-container { padding: 16px; }
    .alert-card { margin: 8px 0; }
    .severity-critical { border-left: 4px solid #f44336; }
    .severity-warning { border-left: 4px solid #ff9800; }
    .severity-info { border-left: 4px solid #2196f3; }
    .severity-icon { margin-right: 8px; }
    .chips { display: flex; gap: 8px; margin-top: 8px; }
    .chip-critical { background-color: #ffebee !important; color: #c62828 !important; }
    .chip-warning { background-color: #fff3e0 !important; color: #e65100 !important; }
    .chip-info { background-color: #e3f2fd !important; color: #1565c0 !important; }
  `]
})
export class AlertsComponent implements OnInit, OnDestroy {
  private readonly api = inject(ApiService);
  private readonly signalR = inject(SignalRService);
  private subscription?: Subscription;
  alerts: AlertDto[] = [];

  ngOnInit(): void {
    this.loadAlerts();
    this.subscription = this.signalR.newAlert$.subscribe(alert => {
      this.alerts = [alert, ...this.alerts];
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  severityIcon(severity: string): string {
    switch (severity) {
      case 'Critical': return '\u26a0\ufe0f';
      case 'Warning': return '\u26a1';
      default: return '\u2139\ufe0f';
    }
  }

  private loadAlerts(): void {
    this.api.getAlerts().subscribe(a => this.alerts = a);
  }
}
