import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

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
  providedIn: 'root'
})
export class ShipmentService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/api/shipments`;

  create(dto: CreateShipmentDto): Observable<ShipmentDto> {
    return this.http.post<ShipmentDto>(this.apiUrl, dto);
  }

  update(id: string, dto: UpdateShipmentDto): Observable<ShipmentDto> {
    return this.http.put<ShipmentDto>(`${this.apiUrl}/${id}`, dto);
  }

  getByOrderId(orderId: string): Observable<ShipmentDto> {
    return this.http.get<ShipmentDto>(`${this.apiUrl}/order/${orderId}`);
  }

  markQcPassed(id: string, qcPassed: boolean, qcNotes?: string): Observable<ShipmentDto> {
    return this.http.patch<ShipmentDto>(`${this.apiUrl}/${id}/qc`, { qcPassed, qcNotes });
  }

  markPacked(id: string): Observable<ShipmentDto> {
    return this.http.patch<ShipmentDto>(`${this.apiUrl}/${id}/packed`, {});
  }
}
