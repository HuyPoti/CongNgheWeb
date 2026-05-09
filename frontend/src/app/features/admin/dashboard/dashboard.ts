import { Component, inject, signal, effect, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { DashboardService, OverviewDto, RevenueChartDto, TopProductDto, TopCustomerDto } from '../../../core/services/dashboard.service';

@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, TranslatePipe],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard implements OnInit {
  private dashboardService = inject(DashboardService);

  overview = signal<OverviewDto | null>(null);
  revenueChart = signal<RevenueChartDto[]>([]);
  topProducts = signal<TopProductDto[]>([]);
  topCustomers = signal<TopCustomerDto[]>([]);
  
  loading = signal(false);
  error = signal<string | null>(null);

  constructor() {
    effect(() => {
      this.loadDashboard();
    });
  }

  ngOnInit() {
    this.loadDashboard();
  }

  loadDashboard() {
    this.loading.set(true);
    this.error.set(null);

    // Load all dashboard data in parallel
    this.dashboardService.getOverview().subscribe({
      next: (overview) => this.overview.set(overview),
      error: (err) => this.error.set('Error loading overview')
    });

    this.dashboardService.getRevenue({ days: 30 }).subscribe({
      next: (data) => this.revenueChart.set(data),
      error: (err) => this.error.set('Error loading revenue')
    });

    this.dashboardService.getTopProducts({ take: 10 }).subscribe({
      next: (data) => this.topProducts.set(data),
      error: (err) => this.error.set('Error loading top products')
    });

    this.dashboardService.getTopCustomers({ take: 10 }).subscribe({
      next: (data) => {
        this.topCustomers.set(data);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Error loading top customers');
        this.loading.set(false);
      }
    });
  }

  // Format currency for display
  formatCurrency(value: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
  }

  // Format number for display
  formatNumber(value: number): string {
    return new Intl.NumberFormat('vi-VN').format(value);
  }
}
