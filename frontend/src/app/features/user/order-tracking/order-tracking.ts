import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { OrderService } from '../../../core/services/order.service';
import { OrderDetailDto, OrderStatusHistoryDto } from '../../../core/models/order.model';
import { ToastService } from '../../../core/services/toast.service';

interface TimelineStep {
  statusName: string;
  time?: Date;
  done: boolean;
  active: boolean;
  icon: string;
  note?: string;
}

@Component({
  selector: 'app-order-tracking',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './order-tracking.html'
})
export class OrderTrackingComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private orderService = inject(OrderService);
  private toast = inject(ToastService);

  order = signal<OrderDetailDto | null>(null);
  timeline = signal<TimelineStep[]>([]);
  isLoading = signal(true);

  // Status mapping for icons
  private statusIcons: Record<string, string> = {
    'pending': 'shopping_cart',
    'confirmed': 'check_circle',
    'processing': 'inventory_2',
    'shipping': 'local_shipping',
    'delivered': 'home',
    'cancelled': 'cancel'
  };

  private statusNames: Record<string, string> = {
    'pending': 'Đã đặt hàng',
    'confirmed': 'Đã xác nhận',
    'processing': 'Đang đóng gói',
    'shipping': 'Đang giao hàng',
    'delivered': 'Giao thành công',
    'cancelled': 'Đã hủy'
  };

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadOrderData(id);
    }
  }

  loadOrderData(id: string) {
    this.isLoading.set(true);
    this.orderService.getById(id).subscribe({
      next: (order: OrderDetailDto) => {
        this.order.set(order);
        this.orderService.getHistory(id).subscribe({
          next: (history: OrderStatusHistoryDto[]) => {
            this.generateTimeline(order, history);
            this.isLoading.set(false);
          },
          error: () => {
            this.toast.error('Không thể tải lịch sử đơn hàng');
            this.isLoading.set(false);
          }
        });
      },
      error: () => {
        this.toast.error('Không thể tải thông tin đơn hàng');
        this.isLoading.set(false);
      }
    });
  }

  generateTimeline(order: OrderDetailDto, history: OrderStatusHistoryDto[]) {
    const steps: TimelineStep[] = [];
    
    // Define the flow
    const flow: (OrderDetailDto['status'])[] = ['pending', 'confirmed', 'processing', 'shipping', 'delivered'];
    
    // If cancelled, replace the flow from the point of cancellation
    const currentStatusIndex = flow.indexOf(order.status);
    
    if (order.status === 'cancelled') {
        // Special case for cancelled
        const cancelHistory = history.find(h => h.newStatusLabel.toLowerCase().includes('hủy') || h.newStatus === 5);
        steps.push({
            statusName: 'Đã hủy',
            time: cancelHistory ? new Date(cancelHistory.createdAt) : undefined,
            done: true,
            active: false,
            icon: 'cancel',
            note: cancelHistory?.note
        });
    } else {
        flow.forEach((status, index) => {
            const statusHistory = history.find(h => h.newStatusLabel === this.statusNames[status] || this.getStatusValue(status) === h.newStatus);
            const isDone = index <= currentStatusIndex;
            const isActive = index === currentStatusIndex;

            steps.push({
                statusName: this.statusNames[status],
                time: statusHistory ? new Date(statusHistory.createdAt) : undefined,
                done: isDone,
                active: isActive,
                icon: this.statusIcons[status],
                note: statusHistory?.note
            });
        });
    }

    this.timeline.set(steps);
  }

  private getStatusValue(status: string): number {
      const map: Record<string, number> = {
          'pending': 1,
          'confirmed': 2,
          'processing': 3,
          'shipping': 4,
          'delivered': 5,
          'cancelled': 6
      };
      return map[status] || 0;
  }

  copyTrackingCode() {
    const code = this.order()?.shipment?.trackingCode;
    if (code) {
      navigator.clipboard.writeText(code);
      this.toast.success('Đã copy mã vận đơn!');
    }
  }
}
