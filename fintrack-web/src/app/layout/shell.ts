import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Auth } from '../core/auth';

@Component({
  selector: 'app-shell',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  template: `
    <div class="app-layout">
      <aside class="sidebar">
        <a class="brand" routerLink="/dashboard">FT<span>FinTrack</span></a>
        <p class="nav-label">MINHAS FINANÇAS</p>
        <nav aria-label="Navegação principal">
          <a routerLink="/dashboard" routerLinkActive="active">Visão geral</a>
          <a routerLink="/transactions" routerLinkActive="active">Movimentações</a>
          <a routerLink="/categories" routerLinkActive="active">Categorias</a>
        </nav>
        <div class="sidebar-bottom"><span>{{ auth.session()?.name }}</span><button type="button" (click)="logout()">Sair da conta</button></div>
      </aside>
      <main class="workspace"><router-outlet /></main>
    </div>
  `,
})
export class Shell {
  readonly auth = inject(Auth);
  private readonly router = inject(Router);
  logout() { this.auth.logout(); void this.router.navigate(['/login']); }
}
