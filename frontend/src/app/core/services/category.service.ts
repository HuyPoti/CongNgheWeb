import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { Category, CreateCategory, UpdateCategory } from '../models/category.model';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private http = inject(HttpClient);
  // environment.apiUrl = 'http://localhost:5165/api'  (đã bao gồm /api)
  private apiUrl = `${environment.apiUrl}/categories`;
  private categoriesCache: Category[] | null = null;

  clearCache(): void {
    this.categoriesCache = null;
  }

  getAll(): Observable<Category[]> {
    if (this.categoriesCache) {
      return of(this.categoriesCache);
    }
    return this.http.get<ApiResponse<Category[]>>(this.apiUrl).pipe(
      map((res) => res.data),
      tap((categories) => {
        this.categoriesCache = categories;
      }),
    );
  }

  getById(id: string): Observable<Category> {
    return this.http.get<ApiResponse<Category>>(`${this.apiUrl}/${id}`).pipe(map((res) => res.data));
  }

  create(category: CreateCategory): Observable<Category> {
    return this.http
      .post<ApiResponse<Category>>(this.apiUrl, category)
      .pipe(
        map((res) => res.data),
        tap(() => this.clearCache())
      );
  }

  update(id: string, category: UpdateCategory): Observable<Category> {
    return this.http
      .put<ApiResponse<Category>>(`${this.apiUrl}/${id}`, category)
      .pipe(
        map((res) => res.data),
        tap(() => this.clearCache())
      );
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(tap(() => this.clearCache()));
  }
}
