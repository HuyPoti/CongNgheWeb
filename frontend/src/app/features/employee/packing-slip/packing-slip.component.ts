import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

@Component({
  selector: 'app-packing-slip',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './packing-slip.component.html',
  styleUrls: ['./packing-slip.component.css']
})
export class PackingSlipComponent {
  private router = inject(Router);
  order = {
    orderId: 'ORD-250426-X89',
    customerName: 'Nguyễn Văn A',
    phone: '0987 654 321',
    address: '123 Đường ABC, Phường 4, Quận Tân Bình, TP. Hồ Chí Minh',
    carrier: 'Giao Hàng Nhanh (GHN)',
    trackingCode: 'GHN123456V8',
    items: [
      { name: 'ASUS ROG Strix RTX 4090 OC Edition', sku: 'ASUS-RTX4090-OC', qty: 1, price: 55900000 },
      { name: 'Nguồn ASUS ROG Thor 1200W Platinum II', sku: 'ASUS-THOR-1200P2', qty: 1, price: 8500000 }
    ],
    total: 64400000,
    note: 'Giao trong giờ hành chính. Vui lòng gọi trước khi giao.'
  };

  print() {
    window.print();
  }

  goBack() {
    this.router.navigate(['/employee/orders']);
  }
}
