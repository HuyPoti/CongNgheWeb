import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import {
  CreateOrderDto,
  OrderDetailDto,
  OrderDto,
  PagedResult,
  UpdateOrderDto,
  OrderStatusHistoryDto,
} from '../models/order.model';
import { ApiResponse } from '../models/api-response.model';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/orders`;

  // GET: /api/orders?status=&page=&pageSize=&userId=
  getAll(status?: string, page = 1, pageSize = 10, userId?: string): Observable<PagedResult<OrderDto>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (status) {
      params = params.set('status', status);
    }
    if (userId) {
      params = params.set('userId', userId);
    }
    return this.http
      .get<ApiResponse<PagedResult<OrderDto>>>(this.baseUrl, { params })
      .pipe(map((res) => res.data));
  }

  // GET: /api/orders/{id}
  getById(id: string): Observable<OrderDetailDto> {
    return this.http
      .get<ApiResponse<OrderDetailDto>>(`${this.baseUrl}/${id}`)
      .pipe(map((res) => res.data));
  }

  // POST: /api/orders
  create(dto: CreateOrderDto): Observable<OrderDetailDto> {
    return this.http
      .post<ApiResponse<OrderDetailDto>>(this.baseUrl, dto)
      .pipe(map((res) => res.data));
  }

  // PUT: /api/orders/{id}
  update(id: string, dto: UpdateOrderDto): Observable<ApiResponse<OrderDetailDto>> {
    return this.http.put<ApiResponse<OrderDetailDto>>(`${this.baseUrl}/${id}`, dto);
  }

  // Helper methods
  updateStatus(id: string, status: UpdateOrderDto['status']) {
    return this.update(id, { status });
  }
  updatePaymentStatus(id: string, paymentStatus: UpdateOrderDto['paymentStatus']) {
    return this.update(id, { paymentStatus });
  }

  // POST: /api/orders/{id}/cancel
  cancel(id: string, reason: string): Observable<ApiResponse<object>> {
    return this.http.post<ApiResponse<object>>(`${this.baseUrl}/${id}/cancel`, { reason });
  }

  // POST: /api/orders/{id}/mark-delivered (Admin only)
  markDelivered(id: string): Observable<ApiResponse<object>> {
    return this.http.post<ApiResponse<object>>(`${this.baseUrl}/${id}/mark-delivered`, {});
  }

  // GET: /api/orders/{id}/history
  getHistory(id: string): Observable<OrderStatusHistoryDto[]> {
    return this.http
      .get<ApiResponse<OrderStatusHistoryDto[]>>(`${this.baseUrl}/${id}/history`)
      .pipe(map((res) => res.data));
  }
}
