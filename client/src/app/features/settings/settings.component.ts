import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { ApiService } from '../../core/services/api.service';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, MatCardModule, MatIconModule],
  template: `
    <div class="settings-container">
      <h2>Instellingen</h2>
      @if (settings) {
        <mat-card>
          <mat-card-header><mat-card-title>Connector Status</mat-card-title></mat-card-header>
          <mat-card-content>
            <pre>{{ settings | json }}</pre>
          </mat-card-content>
        </mat-card>
      } @else {
        <p>Instellingen laden...</p>
      }
    </div>
  `,
  styles: [`.settings-container { padding: 16px; } pre { white-space: pre-wrap; }`]
})
export class SettingsComponent implements OnInit {
  private readonly api = inject(ApiService);
  settings: unknown = null;

  ngOnInit(): void {
    this.api.getSettings().subscribe(s => this.settings = s);
  }
}
