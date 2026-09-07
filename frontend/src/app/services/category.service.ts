import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Category, CreateCategoryRequest, UpdateCategoryRequest } from '../models/category';
import { PagedResult } from '../models/transaction';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = '/api/categories';

  getAll(page = 1, pageSize = 50): Observable<PagedResult<Category>> {
    const params = new HttpParams().set('page', page).set('pageSize', pageSize);
    return this.http.get<PagedResult<Category>>(this.apiUrl, { params });
  }

  create(data: CreateCategoryRequest): Observable<Category> {
    return this.http.post<Category>(this.apiUrl, data);
  }

  update(id: string, data: UpdateCategoryRequest): Observable<Category> {
    return this.http.put<Category>(`${this.apiUrl}/${id}`, data);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
