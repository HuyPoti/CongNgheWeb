import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../../core/services/toast.service';
import { OrderService } from '../../../core/services/order.service';
import { OrderDto, OrderDetailDto, UpdateOrderDto } from '../../../core/models/order.model';

@Component({
  selector: 'app-emp-orders',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './emp-orders.html',
})
export class EmpOrders implements OnInit {
  private orderService = inject(OrderService);
  private toast = inject(ToastService);

  // States
  isLoading = signal(true);
  currentPage = signal(1);
  pageSize = signal(10);
  selectedStatus = signal<string | undefined>(undefined);
  orders = signal<OrderDto[]>([]);
  showDetail = signal<OrderDetailDto | null>(null);
  searchQuery = signal('');

  // Employee chỉ được cập nhật trạng thái theo quy trình tuần tự
  statusFlow: Record<string, NonNullable<UpdateOrderDto['status']>[]> = {
    pending: ['confirmed', 'cancelled'],
    confirmed: ['processing', 'cancelled'],
    processing: ['shipping'],
    shipping: ['delivered'],
    delivered: [],
    cancelled: [],
  };

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
    delivered: 'bg-emerald-500/20 text-emerald-400 border-emerald-500/30',
    cancelled: 'bg-red-500/20 text-red-400 border-red-500/30',
  };

  statusIcons: Record<string, string> = {
    pending: 'schedule',
    confirmed: 'task_alt',
    processing: 'precision_manufacturing',
    shipping: 'local_shipping',
    delivered: 'check_circle',
    cancelled: 'cancel',
  };

  paymentColors: Record<string, string> = {
    unpaid: 'bg-yellow-500/20 text-yellow-400 border-yellow-500/30',
    paid: 'bg-emerald-500/20 text-emerald-400 border-emerald-500/30',
    refunded: 'bg-red-500/20 text-red-400 border-red-500/30',
  };

  paymentLabels: Record<string, string> = {
    unpaid: 'Chưa thanh toán',
    paid: 'Đã thanh toán',
    refunded: 'Đã hoàn tiền',
  };

  filterOptions = ['pending', 'confirmed', 'processing', 'shipping', 'delivered', 'cancelled'];

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

  get filteredOrders(): OrderDto[] {
    const q = this.searchQuery().toLowerCase();
    if (!q) return this.orders();
    return this.orders().filter(
      (o) => o.orderCode.toLowerCase().includes(q) || o.customerName.toLowerCase().includes(q),
    );
  }

  filterByStatus(status: string | undefined) {
    this.selectedStatus.set(status);
    this.loadOrders();
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

  // Employee chỉ được chuyển trạng thái theo quy trình
  getNextStatuses(currentStatus: string): NonNullable<UpdateOrderDto['status']>[] {
    // Chúng ta ép kiểu (Type Casting) kết quả trả về để trình biên dịch yên tâm
    return (this.statusFlow[currentStatus] || []) as NonNullable<UpdateOrderDto['status']>[];
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
          `Đã cập nhật đơn ${order.orderCode} → "${this.statusLabels[newStatus]}"`,
        );
      },
      error: () => this.toast.error('Lỗi cập nhật trạng thái'),
    });
  }

  getOrderStats() {
    const orders = this.orders();
    return {
      total: orders.length,
      pending: orders.filter((o) => o.status === 'pending').length,
      shipping: orders.filter((o) => o.status === 'shipping').length,
      delivered: orders.filter((o) => o.status === 'delivered').length,
    };
  }
}
