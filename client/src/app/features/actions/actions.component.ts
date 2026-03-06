import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { ApiService } from '../../core/services/api.service';
import { ActionItemDto } from '../../core/models';

@Component({
  selector: 'app-actions',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatButtonModule, MatChipsModule, MatButtonToggleModule],
  template: `
    <div class="actions-container">
      <h2>Actie Wachtrij</h2>
      <mat-button-toggle-group [value]="filter" (change)="filterChanged($event.value)">
        <mat-button-toggle value="">Alle</mat-button-toggle>
        <mat-button-toggle value="Pending">Openstaand</mat-button-toggle>
        <mat-button-toggle value="Approved">Goedgekeurd</mat-button-toggle>
        <mat-button-toggle value="Rejected">Afgewezen</mat-button-toggle>
      </mat-button-toggle-group>

      @for (action of actions; track action.id) {
        <mat-card class="action-card">
          <mat-card-header>
            <mat-card-title>{{ action.description }}</mat-card-title>
          </mat-card-header>
          <mat-card-content>
            <div class="chips">
              <mat-chip>{{ action.type }}</mat-chip>
              <mat-chip [class]="'status-' + action.status.toLowerCase()">{{ action.status }}</mat-chip>
            </div>
            <p>{{ action.aiReasoning }}</p>
            @if (action.resolvedAt) {
              <small>Afgehandeld: {{ action.resolvedAt | date:'medium' }} door {{ action.resolvedBy }}</small>
            }
          </mat-card-content>
          @if (action.status === 'Pending') {
            <mat-card-actions>
              <button mat-raised-button color="primary" (click)="approve(action.id)">Goedkeuren</button>
              <button mat-raised-button color="warn" (click)="reject(action.id)">Afwijzen</button>
            </mat-card-actions>
          }
        </mat-card>
      } @empty {
        <p>Geen acties gevonden.</p>
      }
    </div>
  `,
  styles: [`
    .actions-container { padding: 16px; }
    .action-card { margin: 12px 0; }
    .chips { display: flex; gap: 8px; margin: 8px 0; }
    .status-pending { background-color: #fff3e0 !important; }
    .status-approved { background-color: #e8f5e9 !important; }
    .status-rejected { background-color: #ffebee !important; }
    mat-button-toggle-group { margin-bottom: 16px; }
  `]
})
export class ActionsComponent implements OnInit {
  private readonly api = inject(ApiService);
  actions: ActionItemDto[] = [];
  filter = '';

  ngOnInit(): void {
    this.loadActions();
  }

  filterChanged(value: string): void {
    this.filter = value;
    this.loadActions();
  }

  approve(id: number): void {
    this.api.approveAction(id).subscribe(() => this.loadActions());
  }

  reject(id: number): void {
    this.api.rejectAction(id).subscribe(() => this.loadActions());
  }

  private loadActions(): void {
    this.api.getActions(this.filter || undefined).subscribe(a => this.actions = a);
  }
}
