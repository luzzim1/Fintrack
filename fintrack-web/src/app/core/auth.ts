import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, tap, throwError } from 'rxjs';

export interface Session { accessToken: string; expiresAt: string; userId: string; name: string; }

@Injectable({ providedIn: 'root' })
export class Auth {
  private readonly http = inject(HttpClient);
  readonly session = signal<Session | null>(this.restore());
  private restore(): Session | null {
    try {
      const value = JSON.parse(sessionStorage.getItem('fintrack.session') ?? 'null') as Session | null;
      if (value && typeof value.accessToken === 'string' && Date.parse(value.expiresAt) > Date.now()) return value;
    } catch { /* A malformed session is treated as logged out. */ }
    sessionStorage.removeItem('fintrack.session');
    return null;
  }
  isAuthenticated(): boolean {
    const session = this.session();
    if (session && Date.parse(session.expiresAt) > Date.now()) return true;
    this.logout();
    return false;
  }
  login(email: string, password: string) {
    return this.http.post<Session>('/api/auth/login', { email, password }).pipe(tap(session => this.save(session)));
  }
  register(name: string, email: string, password: string) {
    return this.http.post<Session>('/api/auth/register', { name, email, password }).pipe(tap(session => this.save(session)));
  }
  private save(session: Session) {
    sessionStorage.setItem('fintrack.session', JSON.stringify(session));
    this.session.set(session);
  }
  logout() {
    sessionStorage.removeItem('fintrack.session');
    this.session.set(null);
  }
}

export const authGuard: CanActivateFn = () => inject(Auth).isAuthenticated() || inject(Router).createUrlTree(['/login']);

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(Auth);
  const router = inject(Router);
  if (!request.url.startsWith('/api/') || request.url.startsWith('/api/auth/')) return next(request);
  const authenticated = auth.isAuthenticated();
  const token = auth.session()?.accessToken;
  return next(authenticated && token ? request.clone({ setHeaders: { Authorization: 'Bearer ' + token } }) : request).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) { auth.logout(); void router.navigate(['/login']); }
      return throwError(() => error);
    }),
  );
};

export function errorMessage(error: HttpErrorResponse): string {
  if (error.status === 0) return 'Não foi possível conectar à API. Tente novamente.';
  if (error.status === 429) return 'Muitas tentativas. Aguarde um minuto e tente novamente.';
  const details = error.error;
  if (details?.errors) return Object.values(details.errors).flat().join(' ');
  return details?.title ?? 'Não foi possível concluir a operação.';
}
