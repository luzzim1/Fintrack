import { Routes } from '@angular/router';
import { authGuard } from './core/auth';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./pages/auth-page').then(m => m.AuthPage) },
  { path: 'register', loadComponent: () => import('./pages/auth-page').then(m => m.AuthPage) },
  { path: '', canActivate: [authGuard], canActivateChild: [authGuard], loadComponent: () => import('./layout/shell').then(m => m.Shell), children: [
    { path: 'dashboard', loadComponent: () => import('./pages/welcome').then(m => m.Welcome) },
    { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  ] },
  { path: '**', redirectTo: 'dashboard' },
];
