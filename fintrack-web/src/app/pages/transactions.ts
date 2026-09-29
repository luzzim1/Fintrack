import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule, NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize, forkJoin } from 'rxjs';
import { errorMessage } from '../core/auth';
import { Category, FinanceApi, Page, Transaction, TransactionType } from '../core/finance-api';

@Component({
  selector: 'app-transactions',
  imports: [CurrencyPipe, DatePipe, FormsModule, ReactiveFormsModule],
  template: `
    <header class="page-heading"><div><p class="eyebrow">MOVIMENTAÇÕES</p><h1>Receitas e despesas</h1><p class="muted">Registre e encontre rapidamente tudo que movimenta seu saldo.</p></div><button class="primary" type="button" (click)="open()">Nova movimentação</button></header>
    <section class="card filters">
      <label>De<input type="date" [(ngModel)]="filters.from"></label><label>Até<input type="date" [(ngModel)]="filters.to"></label>
      <label>Tipo<select [(ngModel)]="filters.type"><option [ngValue]="null">Todos</option><option [ngValue]="1">Receitas</option><option [ngValue]="2">Despesas</option></select></label>
      <label>Categoria<select [(ngModel)]="filters.categoryId"><option [ngValue]="null">Todas</option>@for (category of categories(); track category.id) { <option [ngValue]="category.id">{{ category.name }}</option> }</select></label>
      <button type="button" (click)="applyFilters()">Filtrar</button>
    </section>
    @if (error()) { <p class="alert error">{{ error() }}</p> }
    @if (editing()) {
      <section class="card form-card" aria-label="Formulário de movimentação">
        <div class="card-title"><h2>{{ form.controls.id.value ? 'Editar movimentação' : 'Nova movimentação' }}</h2><button type="button" (click)="close()">Fechar</button></div>
        <form [formGroup]="form" (ngSubmit)="save()" class="form-grid">
          <label class="wide">Descrição<input formControlName="description" maxlength="500" placeholder="Ex.: Supermercado"></label>
          <label>Valor<input type="number" formControlName="amount" min="0.01" step="0.01"></label>
          <label>Tipo<select formControlName="type" (change)="form.controls.categoryId.setValue(null)"><option [ngValue]="1">Receita</option><option [ngValue]="2">Despesa</option></select></label>
          <label>Data<input type="date" formControlName="date"></label>
          <label>Categoria<select formControlName="categoryId"><option [ngValue]="null">Sem categoria</option>@for (category of compatibleCategories(); track category.id) { <option [ngValue]="category.id">{{ category.name }}</option> }</select></label>
          @if (form.invalid && form.touched) { <p class="field-error wide">Preencha descrição, valor positivo, tipo e data.</p> }
          <div class="form-actions wide"><button type="button" (click)="close()">Cancelar</button><button class="primary" type="submit" [disabled]="saving()">{{ saving() ? 'Salvando…' : 'Salvar' }}</button></div>
        </form>
      </section>
    }
    <section class="card table-card">
      @if (loading()) { <div class="loading">Carregando movimentações…</div> }
      @else if (!page().items.length) { <div class="empty">Nenhuma movimentação encontrada.</div> }
      @else {
        <div class="table-wrap"><table><thead><tr><th>Descrição</th><th>Data</th><th>Categoria</th><th>Tipo</th><th class="right">Valor</th><th></th></tr></thead><tbody>
          @for (item of page().items; track item.id) {
            <tr><td><strong>{{ item.description }}</strong></td><td>{{ item.date | date:'dd/MM/yyyy':'UTC' }}</td><td>{{ item.categoryName || 'Sem categoria' }}</td><td><span class="badge" [class.income-badge]="item.type === 1">{{ item.type === 1 ? 'Receita' : 'Despesa' }}</span></td><td class="right" [class.income]="item.type === 1" [class.expense]="item.type === 2">{{ item.amount | currency:'BRL' }}</td><td class="row-actions"><button type="button" (click)="open(item)">Editar</button><button class="danger" type="button" (click)="remove(item)">Excluir</button></td></tr>
          }
        </tbody></table></div>
        <footer class="pagination"><span>{{ page().totalCount }} resultados · Página {{ page().pageNumber }} de {{ totalPages() }}</span><div><button type="button" [disabled]="page().pageNumber === 1" (click)="go(page().pageNumber - 1)">Anterior</button><button type="button" [disabled]="page().pageNumber >= totalPages()" (click)="go(page().pageNumber + 1)">Próxima</button></div></footer>
      }
    </section>
  `,
})
export class Transactions implements OnInit {
  private readonly api = inject(FinanceApi);
  private readonly fb = inject(NonNullableFormBuilder);
  readonly categories = signal<Category[]>([]);
  readonly page = signal<Page<Transaction>>({ items: [], totalCount: 0, pageNumber: 1, pageSize: 10 });
  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.page().totalCount / this.page().pageSize)));
  readonly editing = signal(false); readonly loading = signal(true); readonly saving = signal(false); readonly error = signal('');
  filters: { from: string; to: string; type: TransactionType | null; categoryId: string | null; page: number; pageSize: number } = { from: '', to: '', type: null, categoryId: null, page: 1, pageSize: 10 };
  readonly form = this.fb.group({ id: [''], description: ['', [Validators.required, Validators.maxLength(500), Validators.pattern(/\S/)]], amount: [0, [Validators.required, Validators.min(.01)]], type: [2 as TransactionType, Validators.required], date: [new Date().toISOString().slice(0, 10), Validators.required], categoryId: [null as string | null] });
  compatibleCategories() { return this.categories().filter(category => category.type === this.form.controls.type.value); }
  ngOnInit() {
    forkJoin({ categories: this.api.categories(), transactions: this.api.transactions(this.filters) }).subscribe({
      next: value => { this.categories.set(value.categories); this.page.set(value.transactions); this.loading.set(false); },
      error: error => { this.error.set(errorMessage(error)); this.loading.set(false); },
    });
  }
  load() { this.loading.set(true); this.api.transactions(this.filters).subscribe({ next: value => { this.page.set(value); this.loading.set(false); }, error: error => { this.error.set(errorMessage(error)); this.loading.set(false); } }); }
  applyFilters() { this.filters.page = 1; this.load(); }
  go(page: number) { this.filters.page = page; this.load(); }
  open(item?: Transaction) {
    this.form.reset(item ? { id: item.id, description: item.description, amount: item.amount, type: item.type, date: item.date, categoryId: item.categoryId } : { id: '', description: '', amount: 0, type: 2, date: new Date().toISOString().slice(0, 10), categoryId: null });
    this.editing.set(true); this.error.set('');
  }
  close() { this.editing.set(false); }
  save() {
    if (this.form.invalid || this.saving()) { this.form.markAllAsTouched(); return; }
    this.saving.set(true); this.error.set('');
    const { id, ...input } = this.form.getRawValue();
    const request = id ? this.api.updateTransaction(id, input) : this.api.createTransaction(input);
    request.pipe(finalize(() => this.saving.set(false))).subscribe({ next: () => { this.close(); this.load(); }, error: error => this.error.set(errorMessage(error)) });
  }
  remove(item: Transaction) {
    if (!confirm(`Excluir "${item.description}"? Esta ação não pode ser desfeita.`)) return;
    this.api.deleteTransaction(item.id).subscribe({ next: () => this.load(), error: error => this.error.set(errorMessage(error)) });
  }
}
