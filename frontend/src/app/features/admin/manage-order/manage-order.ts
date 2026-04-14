import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../../core/services/toast.service';
import { OrderService } from '../../../core/services/order.service';
import { OrderDto, OrderDetailDto, UpdateOrderDto } from '../../../core/models/order.model';

@Component({
  selector: 'app-manage-order',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './manage-order.html',
  styleUrl: './manage-order.css',
})
export class ManageOrder implements OnInit {
  private orderService = inject(OrderService);
  private toast = inject(ToastService);

  // States
  isLoading = signal(true);
  currentPage = signal(1);
  pageSize = signal(10);
  selectedStatus = signal<string | undefined>(undefined);
  orders = signal<OrderDto[]>([]);
  showDetail = signal<OrderDetailDto | null>(null);

  // Options
  statusOptions = [
    'pending',
    'confirmed',
    'processing',
    'shipping',
    'delivered',
    'cancelled',
  ] as const;
  paymentStatusOptions: UpdateOrderDto['paymentStatus'][] = ['unpaid', 'paid', 'refunded'];

  // Labels & Colors
  statusLabels: Record<string, string> = {
    pending: 'Chờ xử lý',
    confirmed: 'Đã xác nhận',
    processing: 'Đang xử lý',
    shipping: 'Đang giao',
    delivered: 'Đã giao',
    cancelled: 'Đã hủy',
  };

  statusColors: Record<string, string> = {
    pending: 'bg-yellow-500/20 text-yellow-400 border-yellow-500/30',
    confirmed: 'bg-blue-500/20 text-blue-400 border-blue-500/30',
    processing: 'bg-purple-500/20 text-purple-400 border-purple-500/30',
    shipping: 'bg-cyan-500/20 text-cyan-400 border-cyan-500/30',
    delivered: 'bg-green-500/20 text-green-400 border-green-500/30',
    cancelled: 'bg-red-500/20 text-red-400 border-red-500/30',
  };

  paymentColors: Record<string, string> = {
    unpaid: 'bg-yellow-500/20 text-yellow-400 border-yellow-500/30',
    paid: 'bg-green-500/20 text-green-400 border-green-500/30',
    refunded: 'bg-red-500/20 text-red-400 border-red-500/30',
  };

  // Life cycle
  ngOnInit() {
    this.loadOrders();
  }

  loadOrders() {
    this.isLoading.set(true);
    this.orderService.getAll(this.selectedStatus(), this.currentPage(), this.pageSize()).subscribe({
      next: (result) => {
        this.orders.set(result.items);
        this.isLoading.set(false);
      },
      error: () => {
        this.toast.error('Không thể tải danh sách đơn hàng');
        this.isLoading.set(false);
      },
    });
  }

  openDetail(order: OrderDto) {
    this.orderService.getById(order.orderId).subscribe({
      next: (detail) => this.showDetail.set(detail),
      error: () => this.toast.error('Không thể tải chi tiết đơn hàng'),
    });
  }

  closeDetail() {
    this.showDetail.set(null);
  }

  updateOrderStatus(order: OrderDto, newStatus: UpdateOrderDto['status']) {
    if (!newStatus) return;

    this.orderService.updateStatus(order.orderId, newStatus).subscribe({
      next: () => {
        this.orders.update((list) =>
          list.map((o) =>
            o.orderId === order.orderId
              ? { ...o, status: newStatus, updatedAt: new Date().toISOString() }
              : o,
          ),
        );
        if (this.showDetail()?.orderId === order.orderId) {
          this.showDetail.set({ ...this.showDetail()!, status: newStatus });
        }
        this.toast.success(
          `Đã cập nhật trạng thái đơn hàng ${order.orderCode} thành "${this.statusLabels[newStatus]}"`,
        );
      },
      error: () => this.toast.error('Lỗi cập nhật trạng thái'),
    });
  }

  updatePaymentStatus(order: OrderDto, newStatus: UpdateOrderDto['paymentStatus']) {
    if (!newStatus) return;

    this.orderService.updatePaymentStatus(order.orderId, newStatus).subscribe({
      next: () => {
        this.orders.update((list) =>
          list.map((o) =>
            o.orderId === order.orderId
              ? { ...o, paymentStatus: newStatus, updatedAt: new Date().toISOString() }
              : o,
          ),
        );
        this.toast.success(`Đã cập nhật trạng thái thanh toán của đơn hàng ${order.orderCode}`);
      },
      error: () => this.toast.error('Lỗi cập nhật thanh toán'),
    });
  }

  getOrderStats() {
    const orders = this.orders();
    return {
      total: orders.length,
      pending: orders.filter((o) => o.status === 'pending').length,
      shipping: orders.filter((o) => o.status === 'shipping').length,
      delivered: orders.filter((o) => o.status === 'delivered').length,
      revenue: orders
        .filter((o) => o.paymentStatus === 'paid')
        .reduce((sum, o) => sum + o.totalAmount, 0),
    };
  }
}
