import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../../core/services/toast.service';

export interface OrderItem {
  order_item_id: number;
  product_id: number;
  product_name: string;
  quantity: number;
  unit_price: number;
}

export interface Payment {
  payment_id: number;
  amount: number;
  payment_method: string;
  transaction_id: string;
  status: 'pending' | 'success' | 'failed';
  paid_at: string | null;
}

export interface Order {
  order_id: number;
  user_id: number;
  customer_name: string;
  order_code: string;
  total_amount: number;
  status: 'pending' | 'confirmed' | 'processing' | 'shipping' | 'delivered' | 'cancelled';
  payment_method: string;
  payment_status: 'unpaid' | 'paid' | 'refunded';
  shipping_address: string;
  notes: string;
  items: OrderItem[];
  payment: Payment | null;
  created_at: string;
  updated_at: string;
}

@Component({
  selector: 'app-manage-order',
  imports: [CommonModule, FormsModule],
  templateUrl: './manage-order.html',
  styleUrl: './manage-order.css',
})
export class ManageOrder {
  private toast = inject(ToastService);
  orders = signal<Order[]>([
    {
      order_id: 1, user_id: 3, customer_name: 'Trần Thị Khách', order_code: 'ORD-20260327-001',
      total_amount: 1139.98, status: 'confirmed', payment_method: 'COD', payment_status: 'unpaid',
      shipping_address: '123 Nguyễn Huệ, Quận 1, TP.HCM', notes: 'Giao buổi sáng',
      items: [
        { order_item_id: 1, product_id: 1, product_name: 'Intel Core i9-14900K', quantity: 1, unit_price: 549.99 },
        { order_item_id: 2, product_id: 2, product_name: 'GeForce RTX 4080 Super', quantity: 1, unit_price: 589.99 }
      ],
      payment: null,
      created_at: '2026-03-27T10:00:00', updated_at: '2026-03-27T10:30:00'
    },
    {
      order_id: 2, user_id: 4, customer_name: 'Lê Văn Gamer', order_code: 'ORD-20260326-002',
      total_amount: 999.99, status: 'shipping', payment_method: 'Banking', payment_status: 'paid',
      shipping_address: '456 Lê Lợi, Quận 3, TP.HCM', notes: '',
      items: [
        { order_item_id: 3, product_id: 2, product_name: 'GeForce RTX 4080 Super', quantity: 1, unit_price: 999.99 }
      ],
      payment: { payment_id: 1, amount: 999.99, payment_method: 'Banking', transaction_id: 'TXN-VCB-20260326-001', status: 'success', paid_at: '2026-03-26T14:00:00' },
      created_at: '2026-03-26T09:00:00', updated_at: '2026-03-27T08:00:00'
    },
    {
      order_id: 3, user_id: 5, customer_name: 'Phạm Minh Tech', order_code: 'ORD-20260325-003',
      total_amount: 139.99, status: 'delivered', payment_method: 'MoMo', payment_status: 'paid',
      shipping_address: '789 Hai Bà Trưng, Quận 1, TP.HCM', notes: 'Gọi trước khi giao',
      items: [
        { order_item_id: 4, product_id: 3, product_name: 'G.Skill Trident Z5 RGB 32GB', quantity: 1, unit_price: 139.99 }
      ],
      payment: { payment_id: 2, amount: 139.99, payment_method: 'MoMo', transaction_id: 'MOMO-20260325-XYZ', status: 'success', paid_at: '2026-03-25T11:00:00' },
      created_at: '2026-03-25T10:00:00', updated_at: '2026-03-26T16:00:00'
    }
  ]);

  showDetail = signal<Order | null>(null);
  statusOptions: Order['status'][] = ['pending', 'confirmed', 'processing', 'shipping', 'delivered', 'cancelled'];
  paymentStatusOptions: Order['payment_status'][] = ['unpaid', 'paid', 'refunded'];

  statusLabels: Record<string, string> = {
    pending: 'Chờ xử lý', confirmed: 'Đã xác nhận', processing: 'Đang xử lý',
    shipping: 'Đang giao', delivered: 'Đã giao', cancelled: 'Đã hủy'
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

  openDetail(order: Order) { this.showDetail.set(order); }
  closeDetail() { this.showDetail.set(null); }

  updateOrderStatus(order: Order, newStatus: Order['status']) {
    this.orders.update(list => list.map(o => o.order_id === order.order_id
      ? { ...o, status: newStatus, updated_at: new Date().toISOString() } : o));
    if (this.showDetail()?.order_id === order.order_id) {
      this.showDetail.set({ ...order, status: newStatus });
    }
    this.toast.success(`Đã cập nhật trạng thái đơn hàng ${order.order_code} thành "${this.statusLabels[newStatus]}"`);
  }

  updatePaymentStatus(order: Order, newStatus: Order['payment_status']) {
    this.orders.update(list => list.map(o => o.order_id === order.order_id
      ? { ...o, payment_status: newStatus, updated_at: new Date().toISOString() } : o));
    this.toast.success(`Đã cập nhật trạng thái thanh toán của đơn hàng ${order.order_code}`);
  }

  getOrderStats() {
    const orders = this.orders();
    return {
      total: orders.length,
      pending: orders.filter(o => o.status === 'pending').length,
      shipping: orders.filter(o => o.status === 'shipping').length,
      delivered: orders.filter(o => o.status === 'delivered').length,
      revenue: orders.filter(o => o.payment_status === 'paid').reduce((sum, o) => sum + o.total_amount, 0)
    };
  }
}
