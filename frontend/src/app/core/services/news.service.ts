import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { News, CreateNews, UpdateNews, NewsCategory, CreateNewsCategory, UpdateNewsCategory } from '../models/news.model';

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
  getNews(): Observable<News[]> { 
    return this.http.get<News[]>(this.apiUrl); 
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