import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
  selector: 'app-vnpay-return',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './vnpay-return.html'
})
export class VnPayReturnComponent implements OnInit {
  isSuccess = true;
  orderCode = 'ORD-250426-X89';
  amount = 55900000;
  transactionId = 'VNP14392841';
  message = 'Giao dịch đã được xác thực thành công.';

  constructor(private route: ActivatedRoute, private router: Router) {}

  ngOnInit() {
    // Mock logic: In reality, we would call paymentService.processVnPayReturn(params)
    this.route.queryParams.subscribe(params => {
      console.log('VNPay Params:', params);
      // Giả lập xử lý kết quả
      if (params['vnp_ResponseCode'] === '00') {
        this.isSuccess = true;
      } else if (params['vnp_ResponseCode']) {
        this.isSuccess = false;
        this.message = 'Giao dịch không thành công hoặc đã bị hủy.';
      }
    });
  }

  viewOrderDetails() {
    this.router.navigate(['/user/orders', this.orderCode]);
  }

  backToHome() {
    this.router.navigate(['/']);
  }
}
