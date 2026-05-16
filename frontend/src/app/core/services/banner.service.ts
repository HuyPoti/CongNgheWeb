import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { Banner, CreateBanner, UpdateBanner } from '../models/banner.model';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

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
    return this.http
      .get<ApiResponse<PagedResult<Banner>>>(this.apiUrl)
      .pipe(map((res) => res.data?.items ?? []));
  }

  getPublic(): Observable<Banner[]> {
    if (this.publicBannersCache) {
      return of(this.publicBannersCache);
    }
    return this.http.get<ApiResponse<PagedResult<Banner>>>(`${this.apiUrl}/public`).pipe(
      map((res) => res.data?.items ?? []),
      tap((banners) => {
        this.publicBannersCache = banners;
      })
    );
  }

  getById(id: string): Observable<Banner> {
    return this.http.get<ApiResponse<Banner>>(`${this.apiUrl}/${id}`).pipe(map((res) => res.data));
  }

  create(banner: CreateBanner): Observable<Banner> {
    return this.http.post<ApiResponse<Banner>>(this.apiUrl, banner).pipe(
      map((res) => res.data),
      tap(() => this.clearCache())
    );
  }

  update(id: string, banner: UpdateBanner): Observable<Banner> {
    return this.http.put<ApiResponse<Banner>>(`${this.apiUrl}/${id}`, banner).pipe(
      map((res) => res.data),
      tap(() => this.clearCache())
    );
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`).pipe(tap(() => this.clearCache()));
  }
}
