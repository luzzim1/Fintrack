import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { Auth, errorMessage } from '../core/auth';

@Component({
  selector: 'app-auth-page',
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <main class="auth-layout">
      <section class="auth-story">
        <a class="brand" routerLink="/">FT<span>FinTrack</span></a>
        <div><p class="eyebrow">SUA VIDA FINANCEIRA, MAIS CLARA</p>
          <h1>Pequenos hábitos.<br>Mais liberdade.</h1>
          <p>Acompanhe suas receitas, entenda seus gastos e organize seus próximos passos.</p>
        </div>
        <span class="subtle">Seu dinheiro. Suas escolhas.</span>
      </section>
      <section class="auth-form">
        <div class="panel">
          <p class="eyebrow">BEM-VINDO AO FINTRACK</p>
          <h2>{{ registering ? 'Comece sua organização' : 'Que bom ter você de volta' }}</h2>
          <p class="muted">{{ registering ? 'Crie sua conta para começar.' : 'Entre para acompanhar suas finanças.' }}</p>
          @if (error()) { <p class="alert error" role="alert">{{ error() }}</p> }
          <form [formGroup]="form" (ngSubmit)="submit()">
            @if (registering) {
              <label>Nome<input formControlName="name" autocomplete="name" maxlength="100" placeholder="Como podemos chamar você?"></label>
              @if (form.controls.name.touched && form.controls.name.invalid) { <small class="field-error">Informe seu nome (até 100 caracteres).</small> }
            }
            <label>Email<input type="email" formControlName="email" autocomplete="email" placeholder="voce@exemplo.com" maxlength="254"></label>
            @if (form.controls.email.touched && form.controls.email.invalid) { <small class="field-error">Informe um email válido.</small> }
            <label>Senha<input type="password" formControlName="password" [autocomplete]="registering ? 'new-password' : 'current-password'" maxlength="128"></label>
            @if (registering) { <small class="muted">Use pelo menos 10 caracteres.</small> }
            @if (form.controls.password.touched && form.controls.password.invalid) { <small class="field-error">Confira o tamanho da senha.</small> }
            <button class="primary full" type="submit" [disabled]="busy()">{{ busy() ? 'Aguarde…' : registering ? 'Criar conta' : 'Entrar' }}</button>
          </form>
          <p class="auth-switch">{{ registering ? 'Já tem uma conta?' : 'Ainda não tem uma conta?' }}
            <a [routerLink]="registering ? '/login' : '/register'">{{ registering ? 'Entrar' : 'Cadastre-se' }}</a>
          </p>
        </div>
      </section>
    </main>
  `,
})
export class AuthPage {
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);
  readonly registering = inject(ActivatedRoute).snapshot.routeConfig?.path === 'register';
  readonly busy = signal(false);
  readonly error = signal('');
  readonly form = inject(NonNullableFormBuilder).group({
    name: ['', this.registering ? [Validators.required, Validators.maxLength(100), Validators.pattern(/\S/)] : []],
    email: ['', [Validators.required, Validators.email, Validators.maxLength(254)]],
    password: ['', [Validators.required, Validators.minLength(this.registering ? 10 : 1), Validators.maxLength(128)]],
  });
  submit() {
    if (this.busy()) return;
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.busy.set(true); this.error.set('');
    const { name, email, password } = this.form.getRawValue();
    const request = this.registering ? this.auth.register(name.trim(), email.trim(), password) : this.auth.login(email.trim(), password);
    request.pipe(finalize(() => this.busy.set(false))).subscribe({
      next: () => void this.router.navigate(['/dashboard']),
      error: error => this.error.set(errorMessage(error)),
    });
  }
}
