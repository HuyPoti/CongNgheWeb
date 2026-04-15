import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
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

@Injectable({ providedIn: 'root' })
export class NewsService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/news`;
  private catUrl = `${environment.apiUrl}/news-categories`;

  // API Danh mục
  getCategories(): Observable<NewsCategory[]> { 
    return this.http.get<NewsCategory[]>(this.catUrl); 
  }
  createCategory(data: CreateNewsCategory): Observable<NewsCategory> { 
    return this.http.post<NewsCategory>(this.catUrl, data); 
  }
  updateCategory(id: string, data: UpdateNewsCategory): Observable<NewsCategory> { 
    return this.http.put<NewsCategory>(`${this.catUrl}/${id}`, data); 
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

    return this.http.get<News[]>(this.apiUrl, { params: httpParams });
  }
  getNewsById(id: string): Observable<News> {
    return this.http.get<News>(`${this.apiUrl}/${id}`);
  }
  createNews(data: CreateNews): Observable<News> { 
    return this.http.post<News>(this.apiUrl, data); 
  }
  updateNews(id: string, data: UpdateNews): Observable<News> { 
    return this.http.put<News>(`${this.apiUrl}/${id}`, data); 
  }
  deleteNews(id: string): Observable<void> { 
    return this.http.delete<void>(`${this.apiUrl}/${id}`); 
  }
}