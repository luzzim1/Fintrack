import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { errorMessage } from '../core/auth';
import { Category, FinanceApi, Summary, Transaction, TransactionType } from '../core/finance-api';

@Component({
  selector: 'app-dashboard',
  imports: [CurrencyPipe, DatePipe, FormsModule],
  template: `
    <header class="page-heading">
      <div><p class="eyebrow">VISÃO GERAL</p><h1>Seu dinheiro em perspectiva</h1><p class="muted">Acompanhe o período e descubra onde ajustar seus próximos passos.</p></div>
    </header>
    <section class="card filters compact">
      <label>De<input type="date" [(ngModel)]="from"></label>
      <label>Até<input type="date" [(ngModel)]="to"></label>
      <label>Tipo<select [(ngModel)]="type"><option [ngValue]="null">Todos</option><option [ngValue]="1">Receitas</option><option [ngValue]="2">Despesas</option></select></label>
      <label>Categoria<select [(ngModel)]="categoryId"><option [ngValue]="null">Todas</option>@for (category of categories(); track category.id) { <option [ngValue]="category.id">{{ category.name }}</option> }</select></label>
      <button class="primary" type="button" (click)="load()">Aplicar filtros</button>
    </section>
    @if (error()) { <p class="alert error">{{ error() }}</p> }
    @if (loading()) { <div class="loading">Carregando seu resumo…</div> }
    @else if (summary(); as value) {
      <section class="stats" aria-label="Resumo financeiro">
        <article class="stat"><span>Receitas</span><strong class="income">{{ value.income | currency:'BRL' }}</strong></article>
        <article class="stat"><span>Despesas</span><strong class="expense">{{ value.expense | currency:'BRL' }}</strong></article>
        <article class="stat accent"><span>Saldo no período</span><strong>{{ value.balance | currency:'BRL' }}</strong><small>{{ value.count }} movimentações</small></article>
      </section>
      <section class="dashboard-grid">
        <article class="card">
          <div class="card-title"><div><p class="eyebrow">DISTRIBUIÇÃO</p><h2>Por categoria</h2></div></div>
          @if (!value.categories.length) { <div class="empty">Nenhuma movimentação no período.</div> }
          @for (item of value.categories; track item.type + ':' + item.categoryId) {
            <div class="category-row"><span class="dot" [class.income-bg]="item.type === 1"></span><span>{{ item.name }}</span><small>{{ item.type === 1 ? 'Receita' : 'Despesa' }}</small><strong>{{ item.amount | currency:'BRL' }}</strong></div>
          }
        </article>
        <article class="card">
          <div class="card-title"><div><p class="eyebrow">ATIVIDADE</p><h2>Movimentações recentes</h2></div></div>
          @if (!recent().length) { <div class="empty">Adicione sua primeira movimentação.</div> }
          @for (item of recent(); track item.id) {
            <div class="transaction-row"><div><strong>{{ item.description }}</strong><small>{{ item.date | date:'dd/MM/yyyy':'UTC' }} · {{ item.categoryName || 'Sem categoria' }}</small></div><strong [class.income]="item.type === 1" [class.expense]="item.type === 2">{{ item.type === 1 ? '+' : '−' }} {{ item.amount | currency:'BRL' }}</strong></div>
          }
        </article>
      </section>
    }
  `,
})
export class Dashboard implements OnInit {
  private readonly api = inject(FinanceApi);
  readonly categories = signal<Category[]>([]);
  readonly summary = signal<Summary | null>(null);
  readonly recent = signal<Transaction[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  from = this.monthStart();
  to = this.isoToday();
  type: TransactionType | null = null;
  categoryId: string | null = null;
  ngOnInit() {
    this.api.categories().subscribe({ next: value => { this.categories.set(value); this.load(); }, error: error => { this.error.set(errorMessage(error)); this.loading.set(false); } });
  }
  load() {
    this.loading.set(true); this.error.set('');
    const filter = { from: this.from, to: this.to, type: this.type, categoryId: this.categoryId };
    forkJoin({ summary: this.api.summary(filter), transactions: this.api.transactions({ ...filter, page: 1, pageSize: 5 }) }).subscribe({
      next: ({ summary, transactions }) => { this.summary.set(summary); this.recent.set(transactions.items); this.loading.set(false); },
      error: error => { this.error.set(errorMessage(error)); this.loading.set(false); },
    });
  }
  private isoToday() { return new Date().toISOString().slice(0, 10); }
  private monthStart() { const value = new Date(); value.setDate(1); return value.toISOString().slice(0, 10); }
}
