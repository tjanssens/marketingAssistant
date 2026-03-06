import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { Subscription } from 'rxjs';
import { ApiService } from '../../core/services/api.service';
import { SignalRService } from '../../core/services/signalr.service';
import { BriefingSummaryDto } from '../../core/models';

@Component({
  selector: 'app-briefings',
  standalone: true,
  imports: [CommonModule, RouterLink, MatCardModule, MatButtonModule, MatListModule],
  template: `
    <div class="briefings-container">
      <div class="header">
        <h2>Briefings</h2>
        <button mat-raised-button color="primary" (click)="generate()">Nieuwe Briefing</button>
      </div>
      <mat-card>
        <mat-list>
          @for (briefing of briefings; track briefing.id) {
            <mat-list-item [routerLink]="['/briefings', briefing.id]">
              <span matListItemTitle>{{ briefing.title }}</span>
              <span matListItemLine>{{ briefing.generatedAt | date:'medium' }} - {{ briefing.period }} - {{ briefing.actionCount }} acties</span>
            </mat-list-item>
          } @empty {
            <mat-list-item>
              <span matListItemTitle>Nog geen briefings</span>
            </mat-list-item>
          }
        </mat-list>
      </mat-card>
    </div>
  `,
  styles: [`
    .briefings-container { padding: 16px; }
    .header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
  `]
})
export class BriefingsComponent implements OnInit, OnDestroy {
  private readonly api = inject(ApiService);
  private readonly signalR = inject(SignalRService);
  private subscription?: Subscription;
  briefings: BriefingSummaryDto[] = [];

  ngOnInit(): void {
    this.loadBriefings();
    this.subscription = this.signalR.newBriefing$.subscribe(briefing => {
      this.briefings = [briefing, ...this.briefings];
    });
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  generate(): void {
    this.api.generateBriefing().subscribe(() => this.loadBriefings());
  }

  private loadBriefings(): void {
    this.api.getBriefings().subscribe(b => this.briefings = b);
  }
}
