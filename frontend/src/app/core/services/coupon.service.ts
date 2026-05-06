import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

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

export interface CouponValidationRequestDto {
  code: string;
  totalAmount: number;
  userId?: string;
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

    return this.http.get<PagedResult<CouponDto>>(this.baseUrl, { params });
  }

  getById(id: string): Observable<CouponDto> {
    return this.http.get<CouponDto>(`${this.baseUrl}/${id}`);
  }

  create(dto: CreateCouponDto): Observable<CouponDto> {
    return this.http.post<CouponDto>(this.baseUrl, dto);
  }

  update(id: string, dto: UpdateCouponDto): Observable<CouponDto> {
    return this.http.put<CouponDto>(`${this.baseUrl}/${id}`, dto);
  }

  deactivate(id: string): Observable<CouponDto> {
    return this.http.delete<CouponDto>(`${this.baseUrl}/${id}`);
  }

  // Public validation (used in checkout/store)
  validate(req: CouponValidationRequestDto): Observable<CouponValidationResultDto> {
    return this.http.post<CouponValidationResultDto>(`${this.baseUrl}/validate`, req);
  }
}
