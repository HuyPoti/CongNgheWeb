import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';

export interface FlashSaleItemDto {
  id: string;
  flashSaleId: string;
  productId: string;
  productName: string;
  flashPrice: number;
  stockLimit: number;
  soldCount: number;
  isSoldOut: boolean;
}

export interface FlashSaleDto {
  flashSaleId: string;
  title: string;
  startTime: string; // ISO datetime
  endTime: string;
  isActive: boolean;
  createdBy?: string;
  createdAt: string;
  items: FlashSaleItemDto[];
}

export interface CreateFlashSaleDto {
  title: string;
  startTime: string;
  endTime: string;
  isActive: boolean;
  createdBy?: string;
}

export interface UpdateFlashSaleDto {
  title?: string;
  startTime?: string;
  endTime?: string;
  isActive?: boolean;
}

export interface CreateFlashSaleItemDto {
  productId: string;
  flashPrice: number;
  stockLimit: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

@Injectable({ providedIn: 'root' })
export class FlashSaleService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/flash-sales`;

  // Admin CRUD
  getAll(opts?: { page?: number; pageSize?: number }): Observable<PagedResult<FlashSaleDto>> {
    const params = new HttpParams()
      .set('page', String(opts?.page ?? 1))
      .set('pageSize', String(opts?.pageSize ?? 10));

    return this.http
      .get<ApiResponse<PagedResult<FlashSaleDto>>>(this.baseUrl, { params })
      .pipe(map((res) => res.data));
  }

  getById(id: string): Observable<FlashSaleDto> {
    return this.http
      .get<ApiResponse<FlashSaleDto>>(`${this.baseUrl}/${id}`)
      .pipe(map((res) => res.data));
  }

  create(dto: CreateFlashSaleDto): Observable<FlashSaleDto> {
    return this.http
      .post<ApiResponse<FlashSaleDto>>(this.baseUrl, dto)
      .pipe(map((res) => res.data));
  }

  update(id: string, dto: UpdateFlashSaleDto): Observable<FlashSaleDto> {
    return this.http
      .put<ApiResponse<FlashSaleDto>>(`${this.baseUrl}/${id}`, dto)
      .pipe(map((res) => res.data));
  }

  // Public endpoint for store/home page
  getActive(): Observable<FlashSaleDto | null> {
    return this.http
      .get<ApiResponse<FlashSaleDto>>(`${this.baseUrl}/active`)
      .pipe(map((res) => res.data));
  }

  // Add item to flash sale
  addItem(flashSaleId: string, dto: CreateFlashSaleItemDto): Observable<FlashSaleItemDto> {
    return this.http
      .post<ApiResponse<FlashSaleItemDto>>(`${this.baseUrl}/${flashSaleId}/items`, dto)
      .pipe(map((res) => res.data));
  }

  // Remove item from flash sale
  removeItem(flashSaleId: string, productId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${flashSaleId}/items/${productId}`);
  }
}
