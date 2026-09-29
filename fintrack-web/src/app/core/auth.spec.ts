import { TestBed } from '@angular/core/testing';
import { provideHttpClient, withInterceptors, HttpClient } from '@angular/common/http';
import { provideHttpClientTesting, HttpTestingController } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { Auth, authGuard, authInterceptor } from './auth';

describe('Authentication', () => {
  let http: HttpTestingController;
  beforeEach(() => {
    sessionStorage.clear();
    TestBed.configureTestingModule({ providers: [provideHttpClient(withInterceptors([authInterceptor])), provideHttpClientTesting(), provideRouter([])] });
    http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => { http.verify(); sessionStorage.clear(); });
  it('stores login only for the current browser session and clears on logout', () => {
    const auth = TestBed.inject(Auth);
    auth.login('ana@example.com', 'password').subscribe();
    http.expectOne('/api/auth/login').flush({ accessToken: 'test', expiresAt: new Date(Date.now() + 60000).toISOString(), userId: 'user', name: 'Ana' });
    expect(auth.isAuthenticated()).toBe(true);
    auth.logout();
    expect(sessionStorage.getItem('fintrack.session')).toBeNull();
  });
  it('does not attach credentials to external requests', () => {
    const auth = TestBed.inject(Auth);
    auth.session.set({ accessToken: 'secret', expiresAt: new Date(Date.now() + 60000).toISOString(), userId: 'user', name: 'Ana' });
    const client = TestBed.inject(HttpClient);
    client.get('/api/transactions').subscribe();
    expect(http.expectOne('/api/transactions').request.headers.get('Authorization')).toBe('Bearer secret');
    client.get('https://example.com/data').subscribe();
    expect(http.expectOne('https://example.com/data').request.headers.has('Authorization')).toBe(false);
  });
  it('rejects expired sessions and guards protected routes', () => {
    const auth = TestBed.inject(Auth);
    auth.session.set({ accessToken: 'expired', expiresAt: '2000-01-01T00:00:00Z', userId: 'user', name: 'Ana' });
    expect(auth.isAuthenticated()).toBe(false);
    expect(TestBed.runInInjectionContext(() => authGuard({} as never, {} as never))).not.toBe(true);
  });
  it('discards malformed saved sessions', () => {
    sessionStorage.setItem('fintrack.session', 'not-json');
    expect(TestBed.inject(Auth).session()).toBeNull();
  });
}
);
