import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Balance } from '../models/balance';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/dashboard';

  getBalance(startDate: string, endDate: string): Observable<Balance> {
    const params = new HttpParams()
      .set('startDate', startDate)
      .set('endDate', endDate);

    return this.http.get<Balance>(`${this.apiUrl}/balance`, { params });
  }
}
