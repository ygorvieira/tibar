import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Balance, MonthlyBalance } from '../models/balance';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/dashboard';

  getBalance(startDate: string, endDate: string, categoryId?: string, type?: string, accountId?: string): Observable<Balance> {
    let params = new HttpParams()
      .set('startDate', startDate)
      .set('endDate', endDate);

    if (categoryId) params = params.set('categoryId', categoryId);
    if (type) params = params.set('type', type);
    if (accountId) params = params.set('accountId', accountId);

    return this.http.get<Balance>(`${this.apiUrl}/balance`, { params });
  }

  getMonthlyBalances(startDate: string, endDate: string, categoryId?: string, type?: string, accountId?: string): Observable<MonthlyBalance[]> {
    let params = new HttpParams()
      .set('startDate', startDate)
      .set('endDate', endDate);

    if (categoryId) params = params.set('categoryId', categoryId);
    if (type) params = params.set('type', type);
    if (accountId) params = params.set('accountId', accountId);

    return this.http.get<MonthlyBalance[]>(`${this.apiUrl}/monthly`, { params });
  }
}
