import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrderDetailDto, OrderDto } from '../../../core/models/order.model';
import { OrderService } from '../../../core/services/order.service';

type OrderFilter = 'all' | 'active' | 'completed' | 'cancelled';

@Component({
  selector: 'app-orders',
  imports: [CommonModule],
  templateUrl: './orders.html',
  styleUrl: './orders.css',
})
export class Orders implements OnInit {
  private orderService = inject(OrderService);

  protected orders = signal<OrderDto[]>([]);
  protected selectedOrderId = signal<string | null>(null);
  protected selectedOrderDetail = signal<OrderDetailDto | null>(null);

  protected isLoading = signal(true);
  protected detailLoading = signal(false);
  protected error = signal('');
  protected selectedFilter = signal<OrderFilter>('all');

  protected readonly timelineSteps = [
    { key: 'pending', label: 'Đã đặt' },
    { key: 'confirmed', label: 'Xác nhận' },
    { key: 'processing', label: 'Đang xử lý' },
    { key: 'shipping', label: 'Đang giao' },
    { key: 'delivered', label: 'Đã giao' },
  ] as const;

  protected filteredOrders = computed(() => {
    const filter = this.selectedFilter();
    const source = this.orders();

    if (filter === 'active') {
      return source.filter((order) => this.isActiveStatus(order.status));
    }
    if (filter === 'completed') {
      return source.filter((order) => order.status === 'delivered');
    }
    if (filter === 'cancelled') {
      return source.filter((order) => order.status === 'cancelled');
    }
    return source;
  });

  protected summary = computed(() => {
    const list = this.orders();
    return {
      total: list.length,
      active: list.filter((order) => this.isActiveStatus(order.status)).length,
      done: list.filter((order) => order.status === 'delivered').length,
    };
  });

  ngOnInit(): void {
    this.loadOrders();
  }

  protected setFilter(filter: OrderFilter): void {
    this.selectedFilter.set(filter);
    this.ensureSelectionFromFilter();
  }

  protected selectOrder(orderId: string): void {
    if (this.selectedOrderId() === orderId) {
      return;
    }

    this.selectedOrderId.set(orderId);
    this.loadOrderDetail(orderId);
  }

  protected stepState(stepKey: string, status: OrderDto['status']): 'done' | 'active' | 'pending' {
    if (status === 'cancelled') {
      return 'pending';
    }

    const currentIndex = this.timelineSteps.findIndex((step) => step.key === status);
    const stepIndex = this.timelineSteps.findIndex((step) => step.key === stepKey);

    if (stepIndex < currentIndex) {
      return 'done';
    }
    if (stepIndex === currentIndex) {
      return 'active';
    }
    return 'pending';
  }

  protected statusLabel(status: OrderDto['status']): string {
    switch (status) {
      case 'pending':
        return 'Chờ xác nhận';
      case 'confirmed':
        return 'Đã xác nhận';
      case 'processing':
        return 'Đang xử lý';
      case 'shipping':
        return 'Đang giao hàng';
      case 'delivered':
        return 'Đã giao thành công';
      case 'cancelled':
        return 'Đã hủy';
      default:
        return status;
    }
  }

  protected paymentLabel(paymentStatus: OrderDto['paymentStatus']): string {
    switch (paymentStatus) {
      case 'paid':
        return 'Đã thanh toán';
      case 'refunded':
        return 'Đã hoàn tiền';
      default:
        return 'Chưa thanh toán';
    }
  }

  protected formatDate(value: string): string {
    return new Date(value).toLocaleString('vi-VN', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  }

  protected fullAddress(detail: OrderDetailDto | null): string {
    if (!detail?.shippingAddress) {
      return 'Không có địa chỉ giao hàng';
    }

    const { addressLine, ward, district, province } = detail.shippingAddress;
    return [addressLine, ward, district, province].filter(Boolean).join(', ');
  }

  protected trackByOrderId(_: number, order: OrderDto): string {
    return order.orderId;
  }

  protected trackByItemId(_: number, item: { orderItemId: string }): string {
    return item.orderItemId;
  }

  private loadOrders(): void {
    this.isLoading.set(true);
    this.error.set('');

    this.orderService.getAll(undefined, 1, 50).subscribe({
      next: (response) => {
        const sorted = [...response.items].sort(
          (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
        );

        this.orders.set(sorted);
        this.isLoading.set(false);
        this.ensureSelectionFromFilter();
      },
      error: () => {
        this.error.set('Không thể tải danh sách đơn hàng. Vui lòng thử lại.');
        this.isLoading.set(false);
      },
    });
  }

  private ensureSelectionFromFilter(): void {
    const list = this.filteredOrders();

    if (list.length === 0) {
      this.selectedOrderId.set(null);
      this.selectedOrderDetail.set(null);
      return;
    }

    const selected = this.selectedOrderId();
    const stillVisible = selected && list.some((order) => order.orderId === selected);

    if (stillVisible) {
      return;
    }

    const nextOrderId = list[0].orderId;
    this.selectedOrderId.set(nextOrderId);
    this.loadOrderDetail(nextOrderId);
  }

  private loadOrderDetail(orderId: string): void {
    this.detailLoading.set(true);

    this.orderService.getById(orderId).subscribe({
      next: (detail) => {
        if (this.selectedOrderId() === orderId) {
          this.selectedOrderDetail.set(detail);
        }
        this.detailLoading.set(false);
      },
      error: () => {
        if (this.selectedOrderId() === orderId) {
          this.selectedOrderDetail.set(null);
        }
        this.detailLoading.set(false);
      },
    });
  }

  private isActiveStatus(status: OrderDto['status']): boolean {
    return ['pending', 'confirmed', 'processing', 'shipping'].includes(status);
  }
}
