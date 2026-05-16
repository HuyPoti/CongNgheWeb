import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiResponse } from '../models/api-response.model';

export interface ReturnRequestItem {
  id: string;
  orderItemId: string;
  productName: string;
  productImageUrl?: string;
  quantity: number;
  unitPrice: number;
  reasonDetail?: string;
}

export interface ReturnRequestImage {
  id: string;
  imageUrl: string;
}

export interface ReturnRequest {
  returnId: string;
  orderId: string;
  orderCode: string;
  userId: string;
  userFullName: string;
  reason: string;
  description?: string;
  status: string;
  refundAmount?: number;
  processedBy?: string;
  processedByName?: string;
  processedAt?: string;
  adminNote?: string;
  createdAt: string;
  updatedAt: string;
  items: ReturnRequestItem[];
  images: ReturnRequestImage[];
}

export interface CreateReturnRequest {
  orderId: string;
  reason: string;
  description?: string;
  items: { orderItemId: string; quantity: number; reasonDetail?: string }[];
  imageUrls: string[];
}

@Injectable({
  providedIn: 'root',
})
export class ReturnRequestService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/returnrequests`;

  getAll(): Observable<ReturnRequest[]> {
    return this.http.get<ApiResponse<ReturnRequest[]>>(this.apiUrl).pipe(map((res) => res.data));
  }

  getById(id: string): Observable<ReturnRequest> {
    return this.http
      .get<ApiResponse<ReturnRequest>>(`${this.apiUrl}/${id}`)
      .pipe(map((res) => res.data));
  }

  getByOrderId(orderId: string): Observable<ReturnRequest> {
    return this.http
      .get<ApiResponse<ReturnRequest>>(`${this.apiUrl}/order/${orderId}`)
      .pipe(map((res) => res.data));
  }

  create(dto: CreateReturnRequest): Observable<ReturnRequest> {
    return this.http.post<ApiResponse<ReturnRequest>>(this.apiUrl, dto).pipe(map((res) => res.data));
  }

  process(
    id: string,
    dto: { status: string; refundAmount?: number; adminNote?: string }
  ): Observable<ReturnRequest> {
    return this.http
      .put<ApiResponse<ReturnRequest>>(`${this.apiUrl}/${id}`, dto)
      .pipe(map((res) => res.data));
  }
}
