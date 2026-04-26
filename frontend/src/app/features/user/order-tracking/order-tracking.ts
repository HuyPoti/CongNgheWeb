import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

interface TimelineStep {
  status: number;
  statusName: string;
  time: Date;
  done: boolean;
  active: boolean;
  icon: string;
  note?: string;
}

@Component({
  selector: 'app-order-tracking',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './order-tracking.html'
})
export class OrderTrackingComponent implements OnInit {
  orderId = 'ORD-250426-X89';
  trackingCode = 'GHN123456V8';
  carrier = 'Giao Hàng Nhanh (GHN)';
  
  timeline: TimelineStep[] = [
    { status: 1, statusName: 'Đã đặt hàng', time: new Date('2026-04-25T08:30:00'), done: true, active: false, icon: 'fa-shopping-cart' },
    { status: 2, statusName: 'Đã xác nhận', time: new Date('2026-04-25T09:15:00'), done: true, active: false, icon: 'fa-check-circle', note: 'Nhân viên: Huy Poti' },
    { status: 3, statusName: 'Đang đóng gói', time: new Date('2026-04-25T10:00:00'), done: true, active: false, icon: 'fa-box', note: 'Đã qua kiểm tra chất lượng (QC Passed)' },
    { status: 4, statusName: 'Đang giao hàng', time: new Date('2026-04-25T13:00:00'), done: false, active: true, icon: 'fa-shipping-fast', note: 'Tài xế: Nguyễn Văn Nam - 0912345xxx' },
    { status: 5, statusName: 'Giao thành công', time: new Date(), done: false, active: false, icon: 'fa-home' }
  ];

  constructor() { }

  ngOnInit(): void {
  }

  copyTrackingCode() {
    navigator.clipboard.writeText(this.trackingCode);
    alert('Đã copy mã vận đơn!');
  }
}
