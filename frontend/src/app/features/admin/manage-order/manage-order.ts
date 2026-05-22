import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../../core/services/toast.service';
import { OrderService } from '../../../core/services/order.service';
import { OrderDto, OrderDetailDto, UpdateOrderDto, OrderStatusHistoryDto } from '../../../core/models/order.model';
import { ShipmentService, ShipmentDto } from '../../../core/services/shipment.service';

@Component({
  selector: 'app-manage-order',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './manage-order.html',
  styleUrl: './manage-order.css',
})
export class ManageOrder implements OnInit {
  private orderService = inject(OrderService);
  private shipmentService = inject(ShipmentService);
  private toast = inject(ToastService);

  // States
  isLoading = signal(true);
  currentPage = signal(1);
  pageSize = signal(10);
  selectedStatus = signal<string | undefined>(undefined);
  orders = signal<OrderDto[]>([]);
  showDetail = signal<OrderDetailDto | null>(null);
  
  // Shipment States
  showShipmentModal = signal(false);
  currentShipment = signal<ShipmentDto | null>(null);
  shipmentForm = signal({ carrier: '' });
  
  // History
  orderHistory = signal<OrderStatusHistoryDto[]>([]);

  // Options
  statusOptions = [
    'pending',
    'confirmed',
    'processing',
    'shipping',
    'delivered',
    'cancelled',
  ] as const;

  // Tránh Admin bấm bypass quy trình Shipment
  getAvailableStatuses(order: OrderDto): string[] {
    const current = order.status;
    if (current === 'pending') return ['pending', 'confirmed', 'cancelled'];
    if (current === 'confirmed') return ['confirmed', 'cancelled']; // Tới processing qua Tạo Shipment
    if (current === 'processing') return ['processing']; // Tới shipping qua cập nhật Mã vận đơn
    if (current === 'shipping') return ['shipping']; // "Đã giao" do Admin xác nhận qua nút riêng
    if (current === 'delivered') return ['delivered'];
    if (current === 'cancelled') return ['cancelled'];
    return [...this.statusOptions];
  }
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
      next: (detail) => {
        this.showDetail.set(detail);
        
        // Load shipment
        this.shipmentService.getByOrderId(order.orderId).subscribe({
          next: (shipment) => this.currentShipment.set(shipment),
          error: () => this.currentShipment.set(null)
        });

        // Load history
        this.orderService.getHistory(order.orderId).subscribe({
          next: (history) => this.orderHistory.set(history),
          error: () => this.orderHistory.set([])
        });
      },
      error: () => this.toast.error('Không thể tải chi tiết đơn hàng'),
    });
  }

  closeDetail() {
    this.showDetail.set(null);
    this.currentShipment.set(null);
    this.orderHistory.set([]);
  }

  // Shipment functions
  openShipmentModal() {
    this.shipmentForm.set({ carrier: '' });
    this.showShipmentModal.set(true);
  }

  closeShipmentModal() {
    this.showShipmentModal.set(false);
  }

  createShipment() {
    const order = this.showDetail();
    if (!order) return;
    
    if (!this.shipmentForm().carrier) {
      this.toast.error('Vui lòng nhập hãng vận chuyển');
      return;
    }

    this.shipmentService.create({
      orderId: order.orderId,
      carrier: this.shipmentForm().carrier
    }).subscribe({
      next: (shipment) => {
        this.currentShipment.set(shipment);
        this.closeShipmentModal();
        this.toast.success('Tạo phiếu giao hàng thành công');
        // Refresh order to get new status
        this.openDetail(order);
        this.loadOrders();
      },
      error: (err) => this.toast.error(err.error?.message || 'Lỗi tạo phiếu giao hàng')
    });
  }

  markQc() {
    const shipment = this.currentShipment();
    if (!shipment) return;

    this.shipmentService.markQcPassed(shipment.shipmentId, true, 'Đã kiểm tra').subscribe({
      next: (s) => {
        this.currentShipment.set(s);
        this.toast.success('Xác nhận QC thành công');
      },
      error: () => this.toast.error('Lỗi QC')
    });
  }

  markPacked() {
    const shipment = this.currentShipment();
    if (!shipment) return;

    this.shipmentService.markPacked(shipment.shipmentId).subscribe({
      next: (s) => {
        this.currentShipment.set(s);
        this.toast.success('Đã đóng gói thành công');
      },
      error: () => this.toast.error('Lỗi đóng gói')
    });
  }

  updateTracking(code: string) {
    const shipment = this.currentShipment();
    if (!shipment) return;

    this.shipmentService.update(shipment.shipmentId, { trackingCode: code }).subscribe({
      next: (s) => {
        this.currentShipment.set(s);
        this.toast.success('Cập nhật mã vận đơn thành công');
        
        // Refresh if order status changed to shipping
        if (this.showDetail()) {
            this.openDetail(this.showDetail()!);
            this.loadOrders();
        }
      },
      error: () => this.toast.error('Lỗi cập nhật mã vận đơn')
    });
  }

  // Kiểm tra đơn COD chưa thu tiền
  isCodUnpaid(order: OrderDetailDto | null): boolean {
    if (!order) return false;
    return order.paymentMethod?.toLowerCase() === 'cod' && order.paymentStatus !== 'paid';
  }

  // Admin xác nhận đã thu tiền COD (dùng nhanh trong modal trước khi giao)
  markAsPaidForCod() {
    const order = this.showDetail();
    if (!order) return;

    this.orderService.updatePaymentStatus(order.orderId, 'paid').subscribe({
      next: () => {
        // Cập nhật local state trong modal
        this.showDetail.set({ ...order, paymentStatus: 'paid' });
        // Cập nhật luôn trong danh sách
        this.orders.update(list =>
          list.map(o => o.orderId === order.orderId
            ? { ...o, paymentStatus: 'paid', updatedAt: new Date().toISOString() }
            : o
          )
        );
        this.toast.success(`Đã xác nhận thu tiền COD cho đơn ${order.orderCode}`);
      },
      error: () => this.toast.error('Lỗi cập nhật trạng thái thanh toán')
    });
  }

  // Admin xác nhận đã giao hàng thành công
  markDelivered() {
    const order = this.showDetail();
    if (!order) return;

    // Guard: Đơn COD phải thu tiền trước
    if (this.isCodUnpaid(order)) {
      this.toast.warning('Đơn COD cần được xác nhận "Đã thu tiền" trước khi đánh dấu đã giao!');
      return;
    }

    this.orderService.markDelivered(order.orderId).subscribe({
      next: () => {
        this.toast.success(`Đơn hàng ${order.orderCode} đã được xác nhận giao thành công!`);
        this.openDetail(order);
        this.loadOrders();
      },
      error: (err) => this.toast.error(err?.error?.message || 'Lỗi xác nhận giao hàng')
    });
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
