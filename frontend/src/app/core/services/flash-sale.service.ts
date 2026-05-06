import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

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
  getAll(opts?: {
    page?: number;
    pageSize?: number;
  }): Observable<PagedResult<FlashSaleDto>> {
    let params = new HttpParams()
      .set('page', String(opts?.page ?? 1))
      .set('pageSize', String(opts?.pageSize ?? 10));

    return this.http.get<PagedResult<FlashSaleDto>>(this.baseUrl, { params });
  }

  getById(id: string): Observable<FlashSaleDto> {
    return this.http.get<FlashSaleDto>(`${this.baseUrl}/${id}`);
  }

  create(dto: CreateFlashSaleDto): Observable<FlashSaleDto> {
    return this.http.post<FlashSaleDto>(this.baseUrl, dto);
  }

  update(id: string, dto: UpdateFlashSaleDto): Observable<FlashSaleDto> {
    return this.http.put<FlashSaleDto>(`${this.baseUrl}/${id}`, dto);
  }

  // Public endpoint for store/home page
  getActive(): Observable<FlashSaleDto | null> {
    return this.http.get<FlashSaleDto>(`${this.baseUrl}/active`);
  }

  // Add item to flash sale
  addItem(flashSaleId: string, dto: CreateFlashSaleItemDto): Observable<FlashSaleItemDto> {
    return this.http.post<FlashSaleItemDto>(`${this.baseUrl}/${flashSaleId}/items`, dto);
  }

  // Remove item from flash sale
  removeItem(flashSaleId: string, productId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${flashSaleId}/items/${productId}`);
  }
}
