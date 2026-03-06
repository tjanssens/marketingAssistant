import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
  },
  {
    path: 'briefings',
    loadComponent: () => import('./features/briefings/briefings.component').then(m => m.BriefingsComponent),
  },
  {
    path: 'briefings/:id',
    loadComponent: () => import('./features/briefings/briefing-detail.component').then(m => m.BriefingDetailComponent),
  },
  {
    path: 'actions',
    loadComponent: () => import('./features/actions/actions.component').then(m => m.ActionsComponent),
  },
  {
    path: 'settings',
    loadComponent: () => import('./features/settings/settings.component').then(m => m.SettingsComponent),
  },
];
