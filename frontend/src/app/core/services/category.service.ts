import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { Category, CreateCategory, UpdateCategory } from '../models/category.model';
import { environment } from '../../../environments/environment';

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
    return this.http.get<Category[]>(this.apiUrl).pipe(
      tap((categories) => {
        this.categoriesCache = categories;
      })
    );
  }

  getById(id: string): Observable<Category> {
    return this.http.get<Category>(`${this.apiUrl}/${id}`);
  }

  create(category: CreateCategory): Observable<Category> {
    return this.http.post<Category>(this.apiUrl, category).pipe(
      tap(() => this.clearCache())
    );
  }

  update(id: string, category: UpdateCategory): Observable<Category> {
    return this.http.put<Category>(`${this.apiUrl}/${id}`, category).pipe(
      tap(() => this.clearCache())
    );
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => this.clearCache())
    );
  }
}
