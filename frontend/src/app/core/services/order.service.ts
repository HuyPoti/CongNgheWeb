import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import {
  ApiResponse,
  CreateOrderDto,
  OrderDetailDto,
  OrderDto,
  PagedResult,
  UpdateOrderDto,
} from '../models/order.model';

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
    return this.http.get<PagedResult<OrderDto>>(this.baseUrl, { params });
  }

  // GET: /api/orders/{id}
  getById(id: string): Observable<OrderDetailDto> {
    return this.http.get<OrderDetailDto>(`${this.baseUrl}/${id}`);
  }

  // POST: /api/orders
  create(dto: CreateOrderDto): Observable<OrderDetailDto> {
    return this.http.post<OrderDetailDto>(this.baseUrl, dto);
  }

  // PUT: /api/orders/{id}
  update(id: string, dto: UpdateOrderDto): Observable<ApiResponse> {
    return this.http.put<ApiResponse>(`${this.baseUrl}/${id}`, dto);
  }

  // Helper methods
  updateStatus(id: string, status: UpdateOrderDto['status']) {
    return this.update(id, { status });
  }
  updatePaymentStatus(id: string, paymentStatus: UpdateOrderDto['paymentStatus']) {
    return this.update(id, { paymentStatus });
  }
}
