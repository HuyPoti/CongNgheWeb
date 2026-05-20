import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { forkJoin } from 'rxjs';
import {
  DashboardService,
  OverviewDto,
  RevenueChartDto,
  TopProductDto,
  TopCustomerDto,
} from '../../../core/services/dashboard.service';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  private dashboardService = inject(DashboardService);

  overview = signal<OverviewDto | null>(null);
  revenueChart = signal<RevenueChartDto[]>([]);
  topProducts = signal<TopProductDto[]>([]);
  topCustomers = signal<TopCustomerDto[]>([]);

  loading = signal(true);
  error = signal<string | null>(null);

  // --- Revenue Chart SVG helpers ---
  /** Tọa độ SVG path cho đường doanh thu (polyline points) */
  revenuePolylinePoints = computed(() => {
    const data = this.revenueChart();
    if (!data.length) return '';
    const W = 800;
    const H = 200;
    const maxRev = Math.max(...data.map((d) => d.revenue), 1);
    return data
      .map((d, i) => {
        const x = (i / (data.length - 1 || 1)) * W;
        const y = H - (d.revenue / maxRev) * H * 0.85;
        return `${x},${y}`;
      })
      .join(' ');
  });

  /** Tọa độ SVG path cho vùng tô bên dưới đường doanh thu */
  revenueAreaPath = computed(() => {
    const pts = this.revenuePolylinePoints();
    if (!pts) return '';
    return `M ${pts.split(' ')[0]} L ${pts.replace(/,/g, ' L ')} L 800,200 L 0,200 Z`;
  });

  /** Labels ngày để hiển thị dưới chart */
  revenueLabels = computed(() => {
    const data = this.revenueChart();
    if (!data.length) return [];
    // Hiển thị tối đa 7 nhãn đều nhau
    const step = Math.max(1, Math.floor(data.length / 7));
    return data
      .filter((_, i) => i % step === 0)
      .map((d) => {
        const date = new Date(d.date);
        return `${date.getDate()}/${date.getMonth() + 1}`;
      });
  });

  ngOnInit() {
    this.loading.set(true);
    this.error.set(null);

    forkJoin({
      overview: this.dashboardService.getOverview(),
      revenue: this.dashboardService.getRevenue({ days: 30 }),
      topProducts: this.dashboardService.getTopProducts({ take: 10 }),
      topCustomers: this.dashboardService.getTopCustomers({ take: 5 }),
    }).subscribe({
      next: (res) => {
        this.overview.set(res.overview);
        this.revenueChart.set(res.revenue);
        this.topProducts.set(res.topProducts);
        this.topCustomers.set(res.topCustomers);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Dashboard load error', err);
        this.error.set('Không thể tải dữ liệu dashboard. Vui lòng thử lại.');
        this.loading.set(false);
      },
    });
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
    }).format(value);
  }

  formatNumber(value: number): string {
    return new Intl.NumberFormat('vi-VN').format(value);
  }
}
