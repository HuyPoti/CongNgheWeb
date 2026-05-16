import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';

export interface ShipmentDto {
  shipmentId: string;
  orderId: string;
  carrier: string;
  trackingCode?: string;
  shippingFee: number;
  estimatedDelivery?: string;
  actualDelivery?: string;
  status: string;
  qcPassed: boolean;
  qcNotes?: string;
  packedBy?: string;
  packedByName?: string;
  packedAt?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateShipmentDto {
  orderId: string;
  carrier: string;
  trackingCode?: string;
  shippingFee?: number;
  estimatedDelivery?: string;
}

export interface UpdateShipmentDto {
  carrier?: string;
  trackingCode?: string;
  shippingFee?: number;
  estimatedDelivery?: string;
  actualDelivery?: string;
}

@Injectable({
  providedIn: 'root',
})
export class ShipmentService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/shipments`;

  create(dto: CreateShipmentDto): Observable<ShipmentDto> {
    return this.http.post<ApiResponse<ShipmentDto>>(this.apiUrl, dto).pipe(map((res) => res.data));
  }

  update(id: string, dto: UpdateShipmentDto): Observable<ShipmentDto> {
    return this.http
      .put<ApiResponse<ShipmentDto>>(`${this.apiUrl}/${id}`, dto)
      .pipe(map((res) => res.data));
  }

  getByOrderId(orderId: string): Observable<ShipmentDto> {
    return this.http
      .get<ApiResponse<ShipmentDto>>(`${this.apiUrl}/order/${orderId}`)
      .pipe(map((res) => res.data));
  }

  markQcPassed(id: string, qcPassed: boolean, qcNotes?: string): Observable<ShipmentDto> {
    return this.http
      .patch<ApiResponse<ShipmentDto>>(`${this.apiUrl}/${id}/qc`, { qcPassed, qcNotes })
      .pipe(map((res) => res.data));
  }

  markPacked(id: string): Observable<ShipmentDto> {
    return this.http
      .patch<ApiResponse<ShipmentDto>>(`${this.apiUrl}/${id}/packed`, {})
      .pipe(map((res) => res.data));
  }
}
