import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule, NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';
import { errorMessage } from '../core/auth';
import { Category, FinanceApi, TransactionType } from '../core/finance-api';

@Component({
  selector: 'app-categories',
  imports: [FormsModule, ReactiveFormsModule],
  template: `
    <header class="page-heading"><div><p class="eyebrow">ORGANIZAÇÃO</p><h1>Categorias</h1><p class="muted">Agrupe suas movimentações para enxergar seus hábitos com clareza.</p></div><button class="primary" type="button" (click)="open()">Nova categoria</button></header>
    @if (error()) { <p class="alert error">{{ error() }}</p> }
    @if (editing()) {
      <section class="card form-card"><div class="card-title"><h2>{{ form.controls.id.value ? 'Editar categoria' : 'Nova categoria' }}</h2><button type="button" (click)="close()">Fechar</button></div>
        <form [formGroup]="form" (ngSubmit)="save()" class="form-grid category-form">
          <label>Nome<input formControlName="name" maxlength="80" placeholder="Ex.: Alimentação"></label>
          <label>Tipo<select formControlName="type"><option [ngValue]="1">Receita</option><option [ngValue]="2">Despesa</option></select></label>
          @if (form.invalid && form.touched) { <p class="field-error wide">Informe um nome de até 80 caracteres.</p> }
          <div class="form-actions wide"><button type="button" (click)="close()">Cancelar</button><button class="primary" type="submit" [disabled]="saving()">Salvar</button></div>
        </form>
      </section>
    }
    <section class="category-grid">
      @if (loading()) { <div class="card loading">Carregando categorias…</div> }
      @else if (!categories().length) { <div class="card empty">Nenhuma categoria cadastrada.</div> }
      @for (category of categories(); track category.id) {
        <article class="card category-card"><span class="badge" [class.income-badge]="category.type === 1">{{ category.type === 1 ? 'Receita' : 'Despesa' }}</span><h2>{{ category.name }}</h2><div><button type="button" (click)="open(category)">Editar</button><button class="danger" type="button" (click)="remove(category)">Excluir</button></div></article>
      }
    </section>
  `,
})
export class Categories implements OnInit {
  private readonly api = inject(FinanceApi); private readonly fb = inject(NonNullableFormBuilder);
  readonly categories = signal<Category[]>([]); readonly editing = signal(false); readonly loading = signal(true); readonly saving = signal(false); readonly error = signal('');
  readonly form = this.fb.group({ id: [''], name: ['', [Validators.required, Validators.maxLength(80), Validators.pattern(/\S/)]], type: [2 as TransactionType, Validators.required] });
  ngOnInit() { this.load(); }
  load() { this.loading.set(true); this.api.categories().subscribe({ next: value => { this.categories.set(value); this.loading.set(false); }, error: error => { this.error.set(errorMessage(error)); this.loading.set(false); } }); }
  open(category?: Category) { this.form.reset(category ? { id: category.id, name: category.name, type: category.type } : { id: '', name: '', type: 2 }); this.editing.set(true); this.error.set(''); }
  close() { this.editing.set(false); }
  save() {
    if (this.form.invalid || this.saving()) { this.form.markAllAsTouched(); return; }
    this.saving.set(true); const { id, ...input } = this.form.getRawValue();
    const request = id ? this.api.updateCategory(id, input) : this.api.createCategory(input);
    request.pipe(finalize(() => this.saving.set(false))).subscribe({ next: () => { this.close(); this.load(); }, error: error => this.error.set(errorMessage(error)) });
  }
  remove(category: Category) {
    if (!confirm(`Excluir a categoria "${category.name}"?`)) return;
    this.api.deleteCategory(category.id).subscribe({ next: () => this.load(), error: error => this.error.set(errorMessage(error)) });
  }
}
