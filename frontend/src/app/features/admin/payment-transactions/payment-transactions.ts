import { Component, OnInit, inject, signal, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PaymentService } from '../../../core/services/payment.service';
import { PaymentTransaction } from '../../../core/models/payment.model';
import { PagedResult } from '../../../core/models/order.model';
import { ToastService } from '../../../core/services/toast.service';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-payment-transactions',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './payment-transactions.html'
})
export class PaymentTransactionsComponent implements OnInit {
  private paymentService = inject(PaymentService);
  private toast = inject(ToastService);
  private platformId = inject(PLATFORM_ID);

  transactions = signal<PaymentTransaction[]>([]);
  totalCount = signal<number>(0);
  page = signal<number>(1);
  pageSize = signal<number>(10);
  keyword = signal<string>('');
  isLoading = signal<boolean>(true);

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.loadTransactions();
    }
  }

  loadTransactions() {
    this.isLoading.set(true);
    this.paymentService.getTransactions({
      keyword: this.keyword(),
      page: this.page(),
      pageSize: this.pageSize()
    }).subscribe({
      next: (res: PagedResult<PaymentTransaction>) => {
        this.transactions.set(res.items);
        this.totalCount.set(res.totalCount);
        this.isLoading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        console.error('Lỗi tải giao dịch:', err);
        this.toast.error('Lỗi khi tải danh sách giao dịch!');
        this.isLoading.set(false);
      }
    });
  }

  onSearch() {
    this.page.set(1);
    this.loadTransactions();
  }

  changePage(newPage: number) {
    if (newPage < 1 || newPage > this.totalPages()) return;
    this.page.set(newPage);
    this.loadTransactions();
  }

  totalPages() {
    return Math.ceil(this.totalCount() / this.pageSize()) || 1;
  }

  getStatusLabel(status: number): string {
    switch (status) {
      case 1: return 'Pending';
      case 2: return 'Success';
      case 3: return 'Failed';
      case 4: return 'Refunded';
      default: return 'Unknown';
    }
  }

  getStatusClass(status: number): string {
    switch (status) {
      case 1: return 'bg-yellow-100 text-yellow-800 border-yellow-200';
      case 2: return 'bg-green-100 text-green-800 border-green-200';
      case 3: return 'bg-red-100 text-red-800 border-red-200';
      case 4: return 'bg-gray-100 text-gray-800 border-gray-200';
      default: return 'bg-gray-100 text-gray-800 border-gray-200';
    }
  }
}
