import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { LoginRequest, RegisterRequest, TokenResponse } from '../models/auth';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/auth';

  readonly currentUser = signal<TokenResponse | null>(null);

  constructor() {
    try {
      const stored = localStorage.getItem('tibar_user');
      if (stored) {
        this.currentUser.set(JSON.parse(stored));
      }
    } catch {
      localStorage.removeItem('tibar_user');
    }
  }

  register(data: RegisterRequest): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(`${this.apiUrl}/register`, data)
      .pipe(tap(res => this.setSession(res)));
  }

  login(data: LoginRequest): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(`${this.apiUrl}/login`, data)
      .pipe(tap(res => this.setSession(res)));
  }

  logout(): void {
    localStorage.removeItem('tibar_user');
    this.currentUser.set(null);
  }

  isAuthenticated(): boolean {
    const user = this.currentUser();
    if (!user) return false;
    return new Date(user.expiresAt) > new Date();
  }

  getToken(): string | null {
    return this.currentUser()?.token ?? null;
  }

  private setSession(res: TokenResponse): void {
    localStorage.setItem('tibar_user', JSON.stringify(res));
    this.currentUser.set(res);
  }
}
