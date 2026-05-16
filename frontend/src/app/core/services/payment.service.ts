import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';

export interface CreatePaymentRequest {
  orderId: string;
  paymentMethod: 'cod' | 'bank_transfer' | 'vnpay';
  returnUrl?: string;
}

export interface PaymentResponse {
  paymentId: string;
  paymentMethod: string;
  status: string;
  paymentUrl: string | null;
  bankInfo: string | null;
}

export interface PaymentDetailResponse {
  paymentId: string;
  orderId: string;
  amount: number;
  paymentMethod: string;
  transactionId: string;
  status: number;
  gatewayResponse: string | null;
  returnUrl: string | null;
  paidAt: string | null;
  createdAt: string;
  updatedAt: string;
}

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/payments`;

  // POST /api/payments
  create(request: CreatePaymentRequest): Observable<PaymentResponse> {
    return this.http
      .post<ApiResponse<PaymentResponse>>(this.baseUrl, request)
      .pipe(map((res) => res.data));
  }

  // GET /api/payments/order/{orderId}
  getByOrderId(orderId: string): Observable<PaymentDetailResponse> {
    return this.http
      .get<ApiResponse<PaymentDetailResponse>>(`${this.baseUrl}/order/${orderId}`)
      .pipe(map((res) => res.data));
  }

  // PATCH /api/payments/{id}/confirm
  confirmPayment(paymentId: string): Observable<{ message: string }> {
    return this.http
      .patch<ApiResponse<{ message: string }>>(`${this.baseUrl}/${paymentId}/confirm`, {})
      .pipe(map((res) => res.data));
  }
}

