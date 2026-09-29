import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

export type TransactionType = 1 | 2;
export interface Category { id: string; name: string; type: TransactionType; }
export interface Transaction { id: string; description: string; amount: number; type: TransactionType; date: string; createdAt: string; categoryId: string | null; categoryName: string | null; }
export interface TransactionInput { description: string; amount: number; type: TransactionType; date: string; categoryId: string | null; }
export interface TransactionFilter { from?: string; to?: string; type?: TransactionType | null; categoryId?: string | null; page?: number; pageSize?: number; }
export interface Page<T> { items: T[]; totalCount: number; pageNumber: number; pageSize: number; }
export interface CategoryTotal { categoryId: string | null; name: string; type: TransactionType; amount: number; }
export interface Summary { income: number; expense: number; balance: number; count: number; categories: CategoryTotal[]; }

@Injectable({ providedIn: 'root' })
export class FinanceApi {
  private readonly http = inject(HttpClient);
  categories() { return this.http.get<Category[]>('/api/categories'); }
  createCategory(input: Omit<Category, 'id'>) { return this.http.post<Category>('/api/categories', input); }
  updateCategory(id: string, input: Omit<Category, 'id'>) { return this.http.put<Category>(`/api/categories/${id}`, input); }
  deleteCategory(id: string) { return this.http.delete<void>(`/api/categories/${id}`); }
  transactions(filter: TransactionFilter) { return this.http.get<Page<Transaction>>('/api/transactions', { params: this.params(filter) }); }
  createTransaction(input: TransactionInput) { return this.http.post<Transaction>('/api/transactions', input); }
  updateTransaction(id: string, input: TransactionInput) { return this.http.put<Transaction>(`/api/transactions/${id}`, input); }
  deleteTransaction(id: string) { return this.http.delete<void>(`/api/transactions/${id}`); }
  summary(filter: TransactionFilter) { return this.http.get<Summary>('/api/dashboard', { params: this.params(filter) }); }
  private params(filter: TransactionFilter): HttpParams {
    let params = new HttpParams();
    for (const [key, value] of Object.entries(filter)) {
      if (value !== undefined && value !== null && value !== '') params = params.set(key, String(value));
    }
    return params;
  }
}
