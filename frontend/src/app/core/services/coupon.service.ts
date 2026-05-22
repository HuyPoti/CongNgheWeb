import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';

export interface CouponDto {
  couponId: string;
  code: string;
  description?: string;
  discountType: string; // 'percentage' | 'fixed'
  discountValue: number;
  minOrderAmount: number;
  maxDiscount?: number;
  usageLimit?: number;
  usedCount: number;
  perUserLimit: number;
  startDate: string; // ISO datetime
  endDate: string;
  isActive: boolean;
  createdBy?: string;
  createdAt: string;
}

export interface CreateCouponDto {
  code: string;
  description?: string;
  discountType: string;
  discountValue: number;
  minOrderAmount: number;
  maxDiscount?: number;
  usageLimit?: number;
  perUserLimit: number;
  startDate: string;
  endDate: string;
  isActive: boolean;
  createdBy?: string;
}

export interface UpdateCouponDto {
  discountType?: string;
  discountValue?: number;
  minOrderAmount?: number;
  maxDiscount?: number;
  usageLimit?: number;
  perUserLimit?: number;
  startDate?: string;
  endDate?: string;
  isActive?: boolean;
}

export interface CouponValidationItemDto {
  productId: string;
  quantity: number;
}

export interface CouponValidationRequestDto {
  code: string;
  totalAmount: number;
  userId?: string;
  items?: CouponValidationItemDto[];
}

export interface CouponValidationResultDto {
  isValid: boolean;
  couponId?: string;
  code?: string;
  discountAmount: number;
  finalAmount: number;
  message: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

@Injectable({ providedIn: 'root' })
export class CouponService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/coupons`;

  // Admin CRUD
  getAll(opts?: {
    page?: number;
    pageSize?: number;
    isActive?: boolean;
    keyword?: string;
  }): Observable<PagedResult<CouponDto>> {
    let params = new HttpParams()
      .set('page', String(opts?.page ?? 1))
      .set('pageSize', String(opts?.pageSize ?? 10));

    if (opts?.isActive !== undefined) params = params.set('isActive', String(opts.isActive));
    if (opts?.keyword?.trim()) params = params.set('keyword', opts.keyword.trim());

    return this.http
      .get<ApiResponse<PagedResult<CouponDto>>>(this.baseUrl, { params })
      .pipe(map((res) => res.data));
  }

  getById(id: string): Observable<CouponDto> {
    return this.http.get<ApiResponse<CouponDto>>(`${this.baseUrl}/${id}`).pipe(map((res) => res.data));
  }

  getByCode(code: string): Observable<CouponDto> {
    return this.http.get<ApiResponse<CouponDto>>(`${this.baseUrl}/code/${code}`).pipe(map((res) => res.data));
  }

  create(dto: CreateCouponDto): Observable<CouponDto> {
    return this.http.post<ApiResponse<CouponDto>>(this.baseUrl, dto).pipe(map((res) => res.data));
  }

  update(id: string, dto: UpdateCouponDto): Observable<CouponDto> {
    return this.http
      .put<ApiResponse<CouponDto>>(`${this.baseUrl}/${id}`, dto)
      .pipe(map((res) => res.data));
  }

  deactivate(id: string): Observable<CouponDto> {
    return this.http.delete<ApiResponse<CouponDto>>(`${this.baseUrl}/${id}`).pipe(map((res) => res.data));
  }

  // Public validation (used in checkout/store)
  validate(req: CouponValidationRequestDto): Observable<CouponValidationResultDto> {
    return this.http
      .post<ApiResponse<CouponValidationResultDto>>(`${this.baseUrl}/validate`, req)
      .pipe(map((res) => res.data));
  }

  getActiveCoupons(): Observable<PagedResult<CouponDto>> {
    return this.http
      .get<ApiResponse<PagedResult<CouponDto>>>(`${this.baseUrl}/active`)
      .pipe(map((res) => res.data));
  }
}
