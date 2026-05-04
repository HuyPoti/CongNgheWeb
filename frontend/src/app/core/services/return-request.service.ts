import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

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
  providedIn: 'root'
})
export class ReturnRequestService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/returnrequests`;

  getAll(): Observable<ReturnRequest[]> {
    return this.http.get<ReturnRequest[]>(this.apiUrl);
  }

  getById(id: string): Observable<ReturnRequest> {
    return this.http.get<ReturnRequest>(`${this.apiUrl}/${id}`);
  }

  getByOrderId(orderId: string): Observable<ReturnRequest> {
    return this.http.get<ReturnRequest>(`${this.apiUrl}/order/${orderId}`);
  }

  create(dto: CreateReturnRequest): Observable<ReturnRequest> {
    return this.http.post<ReturnRequest>(this.apiUrl, dto);
  }

  process(id: string, dto: { status: string; refundAmount?: number; adminNote?: string }): Observable<ReturnRequest> {
    return this.http.put<ReturnRequest>(`${this.apiUrl}/${id}`, dto);
  }
}
