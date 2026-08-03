import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Account, CreateAccountRequest, UpdateAccountRequest } from '../models/account';

@Injectable({ providedIn: 'root' })
export class AccountService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/accounts';

  getAll(): Observable<Account[]> {
    return this.http.get<Account[]>(this.apiUrl);
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
