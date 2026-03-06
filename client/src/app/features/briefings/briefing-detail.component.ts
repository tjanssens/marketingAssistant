import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatButtonModule } from '@angular/material/button';
import { ApiService } from '../../core/services/api.service';
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
export class BriefingDetailComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  briefing: BriefingDto | null = null;

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.api.getBriefing(id).subscribe(b => this.briefing = b);
  }

  approve(actionId: number): void {
    this.api.approveAction(actionId).subscribe(() => this.ngOnInit());
  }

  reject(actionId: number): void {
    this.api.rejectAction(actionId).subscribe(() => this.ngOnInit());
  }
}
