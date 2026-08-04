import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Transaction, CreateTransactionRequest, UpdateTransactionRequest, PagedResult } from '../models/transaction';

@Injectable({ providedIn: 'root' })
export class TransactionService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/transactions';

  getByPeriod(startDate: string, endDate: string, categoryId?: string, type?: string, accountId?: string): Observable<PagedResult<Transaction>> {
    let params = new HttpParams()
      .set('startDate', startDate)
      .set('endDate', endDate);

    if (categoryId) params = params.set('categoryId', categoryId);
    if (type) params = params.set('type', type);
    if (accountId) params = params.set('accountId', accountId);

    return this.http.get<PagedResult<Transaction>>(this.apiUrl, { params });
  }

  create(data: CreateTransactionRequest): Observable<Transaction[]> {
    return this.http.post<Transaction[]>(this.apiUrl, data);
  }

  update(id: string, data: UpdateTransactionRequest): Observable<Transaction> {
    return this.http.put<Transaction>(`${this.apiUrl}/${id}`, data);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
