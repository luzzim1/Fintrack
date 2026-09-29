import { Routes } from '@angular/router';
import { authGuard } from './core/auth';

export const routes: Routes = [
  { path: 'login', loadComponent: () => import('./pages/auth-page').then(m => m.AuthPage) },
  { path: 'register', loadComponent: () => import('./pages/auth-page').then(m => m.AuthPage) },
  { path: '', canActivate: [authGuard], canActivateChild: [authGuard], loadComponent: () => import('./layout/shell').then(m => m.Shell), children: [
    { path: 'dashboard', loadComponent: () => import('./pages/dashboard').then(m => m.Dashboard) },
    { path: 'transactions', loadComponent: () => import('./pages/transactions').then(m => m.Transactions) },
    { path: 'categories', loadComponent: () => import('./pages/categories').then(m => m.Categories) },
    { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  ] },
  { path: '**', redirectTo: 'dashboard' },
];
