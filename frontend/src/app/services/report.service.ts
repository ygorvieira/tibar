import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ExpenseReport } from '../models/report';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/reports';

  getExpensesByCategory(startDate: string, endDate: string, accountId?: string): Observable<ExpenseReport> {
    let params = new HttpParams()
      .set('startDate', startDate)
      .set('endDate', endDate);

    if (accountId) params = params.set('accountId', accountId);

    return this.http.get<ExpenseReport>(`${this.apiUrl}/expenses-by-category`, { params });
  }
}
