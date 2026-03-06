import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatListModule } from '@angular/material/list';
import { SignalRService } from './core/services/signalr.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    RouterOutlet, RouterLink, RouterLinkActive,
    MatToolbarModule, MatButtonModule, MatIconModule, MatSidenavModule, MatListModule,
  ],
  template: `
    <mat-toolbar color="primary">
      <span>Marketing Assistent</span>
      <nav>
        <a mat-button routerLink="/dashboard" routerLinkActive="active">Dashboard</a>
        <a mat-button routerLink="/briefings" routerLinkActive="active">Briefings</a>
        <a mat-button routerLink="/alerts" routerLinkActive="active">Alerts</a>
        <a mat-button routerLink="/actions" routerLinkActive="active">Acties</a>
        <a mat-button routerLink="/settings" routerLinkActive="active">Instellingen</a>
      </nav>
    </mat-toolbar>
    <main>
      <router-outlet />
    </main>
  `,
  styles: [`
    mat-toolbar {
      display: flex;
      gap: 16px;
    }
    mat-toolbar nav {
      display: flex;
      gap: 4px;
      margin-left: 24px;
    }
    .active { font-weight: bold; }
    main { max-width: 1200px; margin: 0 auto; }
  `]
})
export class AppComponent implements OnInit {
  private readonly signalR = inject(SignalRService);

  ngOnInit(): void {
    this.signalR.start();
  }
}
