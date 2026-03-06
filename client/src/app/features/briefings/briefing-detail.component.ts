import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatButtonModule } from '@angular/material/button';
import { Subscription } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import { SignalRService } from '../../core/services/signalr.service';
import { BriefingDto } from '../../core/models';

@Component({
  selector: 'app-briefing-detail',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatChipsModule, MatButtonModule],
  template: `
    @if (briefing) {
      <div class="briefing-detail">
        <mat-card>
          <mat-card-header>
            <mat-card-title>{{ briefing.title }}</mat-card-title>
            <mat-card-subtitle>{{ briefing.generatedAt | date:'medium' }} - {{ briefing.period }}</mat-card-subtitle>
          </mat-card-header>
          <mat-card-content>
            <div class="content" [innerHTML]="briefing.content"></div>
          </mat-card-content>
        </mat-card>

        @if (briefing.actions.length > 0) {
          <h3>Voorgestelde Acties</h3>
          @for (action of briefing.actions; track action.id) {
            <mat-card class="action-card">
              <mat-card-header>
                <mat-card-title>{{ action.description }}</mat-card-title>
                <mat-chip>{{ action.type }}</mat-chip>
                <mat-chip [class]="'status-' + action.status.toLowerCase()">{{ action.status }}</mat-chip>
              </mat-card-header>
              <mat-card-content>
                <p>{{ action.aiReasoning }}</p>
              </mat-card-content>
              @if (action.status === 'Pending') {
                <mat-card-actions>
                  <button mat-raised-button color="primary" (click)="approve(action.id)">Goedkeuren</button>
                  <button mat-raised-button color="warn" (click)="reject(action.id)">Afwijzen</button>
                </mat-card-actions>
              }
            </mat-card>
          }
        }
      </div>
    } @else {
      <p>Briefing laden...</p>
    }
  `,
  styles: [`
    .briefing-detail { padding: 16px; }
    .content { white-space: pre-wrap; margin: 16px 0; }
    .action-card { margin: 8px 0; }
    .status-pending { background-color: #fff3e0 !important; }
    .status-approved { background-color: #e8f5e9 !important; }
    .status-rejected { background-color: #ffebee !important; }
  `]
})
export class BriefingDetailComponent implements OnInit, OnDestroy {
  private readonly api = inject(ApiService);
  private readonly signalR = inject(SignalRService);
  private readonly route = inject(ActivatedRoute);
  private subscription?: Subscription;
  briefing: BriefingDto | null = null;

  ngOnInit(): void {
    this.loadBriefing();
    this.subscription = this.signalR.actionUpdated$.subscribe(updated => {
      if (!this.briefing) return;
      const idx = this.briefing.actions.findIndex(a => a.id === updated.id);
      if (idx >= 0) {
        this.briefing.actions[idx] = updated;
      }
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  approve(actionId: number): void {
    this.api.approveAction(actionId).subscribe();
  }

  reject(actionId: number): void {
    this.api.rejectAction(actionId).subscribe();
  }

  private loadBriefing(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.api.getBriefing(id).subscribe(b => this.briefing = b);
  }
}
