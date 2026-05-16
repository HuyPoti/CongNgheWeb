import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';

export interface OverviewDto {
  totalRevenue: number;
  totalOrders: number;
  totalCustomers: number;
  activeCoupons: number;
  activeFlashSales: number;
}

export interface RevenueChartDto {
  date: string; // ISO date
  revenue: number;
  orderCount: number;
}

export interface TopProductDto {
  productId: string;
  productName: string;
  unitsSold: number;
  revenue: number;
}

export interface TopCustomerDto {
  userId: string;
  fullName: string;
  totalOrders: number;
  totalSpent: number;
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/dashboard`;

  getOverview(): Observable<OverviewDto> {
    return this.http
      .get<ApiResponse<OverviewDto>>(`${this.baseUrl}/overview`)
      .pipe(map((res) => res.data));
  }

  getRevenue(opts?: { days?: number }): Observable<RevenueChartDto[]> {
    const params = new HttpParams().set('days', String(opts?.days ?? 30));

    return this.http
      .get<ApiResponse<RevenueChartDto[]>>(`${this.baseUrl}/revenue`, { params })
      .pipe(map((res) => res.data));
  }

  getTopProducts(opts?: { take?: number }): Observable<TopProductDto[]> {
    const params = new HttpParams().set('take', String(opts?.take ?? 10));

    return this.http
      .get<ApiResponse<TopProductDto[]>>(`${this.baseUrl}/top-products`, { params })
      .pipe(map((res) => res.data));
  }

  getTopCustomers(opts?: { take?: number }): Observable<TopCustomerDto[]> {
    const params = new HttpParams().set('take', String(opts?.take ?? 10));

    return this.http
      .get<ApiResponse<TopCustomerDto[]>>(`${this.baseUrl}/top-customers`, { params })
      .pipe(map((res) => res.data));
  }
}
