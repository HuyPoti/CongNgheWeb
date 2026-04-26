import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-coupon-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './coupon-list.html'
})
export class CouponListComponent {
  mockCoupons = [
    { id: 1, code: 'HELLOSUMMER', type: 'Phần trăm', value: '10%', minOrder: 1000000, used: 15, limit: 100, expiry: '30/06/2026', active: true },
    { id: 2, code: 'GEARVNVIP', type: 'Cố định', value: '500,000đ', minOrder: 5000000, used: 5, limit: 20, expiry: '31/12/2026', active: true },
    { id: 3, code: 'FREETICKET', type: 'Phần trăm', value: '100%', minOrder: 0, used: 50, limit: 50, expiry: '20/04/2026', active: false }
  ];

  deleteCoupon(id: number) {
    if(confirm('Bạn có chắc muốn vô hiệu hóa mã này?')) {
      const c = this.mockCoupons.find(x => x.id === id);
      if(c) c.active = false;
    }
  }
}
