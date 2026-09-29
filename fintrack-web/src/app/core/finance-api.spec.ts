import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { FinanceApi } from './finance-api';

describe('FinanceApi', () => {
  let api: FinanceApi; let http: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideHttpClientTesting()] });
    api = TestBed.inject(FinanceApi); http = TestBed.inject(HttpTestingController);
  });
  afterEach(() => http.verify());
  it('serializes only selected transaction filters', () => {
    api.transactions({ from: '2026-01-01', type: null, page: 2, pageSize: 10 }).subscribe();
    const request = http.expectOne(value => value.url === '/api/transactions');
    expect(request.request.params.get('from')).toBe('2026-01-01');
    expect(request.request.params.get('page')).toBe('2');
    expect(request.request.params.has('type')).toBe(false);
    request.flush({ items: [], totalCount: 0, pageNumber: 2, pageSize: 10 });
  });
  it('uses the correct update and delete category endpoints', () => {
    api.updateCategory('category-1', { name: 'Lazer', type: 2 }).subscribe();
    const update = http.expectOne('/api/categories/category-1'); expect(update.request.method).toBe('PUT'); update.flush({});
    api.deleteCategory('category-1').subscribe();
    const remove = http.expectOne('/api/categories/category-1'); expect(remove.request.method).toBe('DELETE'); remove.flush(null);
  });
  it('posts transaction input without an id', () => {
    api.createTransaction({ description: 'Salário', amount: 100, type: 1, date: '2026-01-01', categoryId: null }).subscribe();
    const request = http.expectOne('/api/transactions');
    expect(request.request.method).toBe('POST'); expect(request.request.body.description).toBe('Salário'); expect(request.request.body.id).toBeUndefined();
    request.flush({});
  });
});
