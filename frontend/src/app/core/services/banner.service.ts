import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Banner, CreateBanner, UpdateBanner } from '../models/banner.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class BannerService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/banners`;

  getAll(): Observable<Banner[]> {
    return this.http.get<Banner[]>(this.apiUrl);
  }

  getPublic(): Observable<Banner[]> {
    return this.http.get<Banner[]>(`${this.apiUrl}/public`);
  }

  getById(id: string): Observable<Banner> {
    return this.http.get<Banner>(`${this.apiUrl}/${id}`);
  }

  create(banner: CreateBanner): Observable<Banner> {
    return this.http.post<Banner>(this.apiUrl, banner);
  }

  update(id: string, banner: UpdateBanner): Observable<Banner> {
    return this.http.put<Banner>(`${this.apiUrl}/${id}`, banner);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
