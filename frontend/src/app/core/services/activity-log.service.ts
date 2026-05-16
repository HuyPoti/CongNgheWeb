import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';

export interface ActivityLogDto {
  logId: string;
  userId: string;
  userName?: string;
  action: string;
  entityType?: string;
  entityId?: string;
  oldValue?: string;
  newValue?: string;
  ipAddress?: string;
  createdAt: string;
}

export interface ActivityLogQueryDto {
  page?: number;
  pageSize?: number;
  userId?: string;
  entityType?: string;
  fromDate?: string;
  toDate?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

@Injectable({ providedIn: 'root' })
export class ActivityLogService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/activity-logs`;

  getAll(opts?: ActivityLogQueryDto): Observable<PagedResult<ActivityLogDto>> {
    let params = new HttpParams()
      .set('page', String(opts?.page ?? 1))
      .set('pageSize', String(opts?.pageSize ?? 20));

    if (opts?.userId) params = params.set('userId', opts.userId);
    if (opts?.entityType?.trim()) params = params.set('entityType', opts.entityType.trim());
    if (opts?.fromDate) params = params.set('from', opts.fromDate);
    if (opts?.toDate) params = params.set('to', opts.toDate);

    return this.http
      .get<ApiResponse<PagedResult<ActivityLogDto>>>(this.baseUrl, { params })
      .pipe(map((res) => res.data));
  }
}
