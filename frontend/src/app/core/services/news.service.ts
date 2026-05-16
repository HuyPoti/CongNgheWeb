import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  News,
  CreateNews,
  UpdateNews,
  NewsCategory,
  CreateNewsCategory,
  UpdateNewsCategory,
  NewsQueryParams,
} from '../models/news.model';
import { ApiResponse } from '../models/api-response.model';

@Injectable({ providedIn: 'root' })
export class NewsService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/news`;
  private catUrl = `${environment.apiUrl}/news-categories`;

  // API Danh mục
  getCategories(): Observable<NewsCategory[]> {
    return this.http.get<ApiResponse<NewsCategory[]>>(this.catUrl).pipe(map((res) => res.data));
  }
  createCategory(data: CreateNewsCategory): Observable<NewsCategory> {
    return this.http
      .post<ApiResponse<NewsCategory>>(this.catUrl, data)
      .pipe(map((res) => res.data));
  }
  updateCategory(id: string, data: UpdateNewsCategory): Observable<NewsCategory> {
    return this.http
      .put<ApiResponse<NewsCategory>>(`${this.catUrl}/${id}`, data)
      .pipe(map((res) => res.data));
  }
  deleteCategory(id: string): Observable<void> {
    return this.http.delete<void>(`${this.catUrl}/${id}`);
  }

  // API Tin tức
  getNews(params?: NewsQueryParams): Observable<News[]> {
    let httpParams = new HttpParams();

    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') {
          httpParams = httpParams.set(key, String(value));
        }
      });
    }

    return this.http
      .get<ApiResponse<{ items: News[] }>>(this.apiUrl, { params: httpParams })
      .pipe(map((res) => res.data.items));
  }
  getNewsById(id: string): Observable<News> {
    return this.http.get<ApiResponse<News>>(`${this.apiUrl}/${id}`).pipe(map((res) => res.data));
  }
  createNews(data: CreateNews): Observable<News> {
    return this.http.post<ApiResponse<News>>(this.apiUrl, data).pipe(map((res) => res.data));
  }
  updateNews(id: string, data: UpdateNews): Observable<News> {
    return this.http
      .put<ApiResponse<News>>(`${this.apiUrl}/${id}`, data)
      .pipe(map((res) => res.data));
  }
  deleteNews(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}