import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { Banner, CreateBanner, UpdateBanner } from '../models/banner.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class BannerService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/banners`;
  private publicBannersCache: Banner[] | null = null;

  clearCache(): void {
    this.publicBannersCache = null;
  }

  getAll(): Observable<Banner[]> {
    return this.http.get<Banner[]>(this.apiUrl);
  }

  getPublic(): Observable<Banner[]> {
    if (this.publicBannersCache) {
      return of(this.publicBannersCache);
    }
    return this.http.get<Banner[]>(`${this.apiUrl}/public`).pipe(
      tap((banners) => {
        this.publicBannersCache = banners;
      })
    );
  }

  getById(id: string): Observable<Banner> {
    return this.http.get<Banner>(`${this.apiUrl}/${id}`);
  }

  create(banner: CreateBanner): Observable<Banner> {
    return this.http.post<Banner>(this.apiUrl, banner).pipe(
      tap(() => this.clearCache())
    );
  }

  update(id: string, banner: UpdateBanner): Observable<Banner> {
    return this.http.put<Banner>(`${this.apiUrl}/${id}`, banner).pipe(
      tap(() => this.clearCache())
    );
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(
      tap(() => this.clearCache())
    );
  }
}
