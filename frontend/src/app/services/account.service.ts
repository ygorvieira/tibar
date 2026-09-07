import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Account, CreateAccountRequest, UpdateAccountRequest } from '../models/account';
import { PagedResult } from '../models/transaction';

@Injectable({ providedIn: 'root' })
export class AccountService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/accounts';

  getAll(page = 1, pageSize = 50): Observable<PagedResult<Account>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<Account>>(this.apiUrl, { params });
  }

  create(data: CreateAccountRequest): Observable<Account> {
    return this.http.post<Account>(this.apiUrl, data);
  }

  update(id: string, data: UpdateAccountRequest): Observable<Account> {
    return this.http.put<Account>(`${this.apiUrl}/${id}`, data);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
