import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

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
    return this.http.get<OverviewDto>(`${this.baseUrl}/overview`);
  }

  getRevenue(opts?: { days?: number }): Observable<RevenueChartDto[]> {
    let params = new HttpParams()
      .set('days', String(opts?.days ?? 30));

    return this.http.get<RevenueChartDto[]>(`${this.baseUrl}/revenue`, { params });
  }

  getTopProducts(opts?: { take?: number }): Observable<TopProductDto[]> {
    let params = new HttpParams()
      .set('take', String(opts?.take ?? 10));

    return this.http.get<TopProductDto[]>(`${this.baseUrl}/top-products`, { params });
  }

  getTopCustomers(opts?: { take?: number }): Observable<TopCustomerDto[]> {
    let params = new HttpParams()
      .set('take', String(opts?.take ?? 10));

    return this.http.get<TopCustomerDto[]>(`${this.baseUrl}/top-customers`, { params });
  }
}
