import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { PaymentService } from '../../../core/services/payment.service';
import { CartService } from '../../../core/services/cart.service';

@Component({
  selector: 'app-vnpay-return',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './vnpay-return.html'
})
export class VnPayReturnComponent implements OnInit {
  private paymentService = inject(PaymentService);
  private cartService = inject(CartService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  isSuccess = false;
  orderCode = 'Đang tải...';
  amount = 0;
  transactionId = '';
  message = 'Đang xử lý kết quả thanh toán từ VNPAY...';



  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      console.log('VNPay Callback Params:', params);
      
      const vnpParams: Record<string, string | number | boolean | readonly (string | number | boolean)[]> = {};
      Object.keys(params).forEach(key => {
        vnpParams[key] = params[key];
      });

      // Lấy số tiền từ VNPAY param và chia cho 100
      const vnpAmount = params['vnp_Amount'];
      if (vnpAmount) {
        this.amount = Number(vnpAmount) / 100;
      }

      this.transactionId = params['vnp_TransactionNo'] || '';

      this.paymentService.processVnPayReturn(vnpParams).subscribe({
        next: (res) => {
          this.isSuccess = res.success;
          this.message = res.message;
          if (res.success) {
            this.message = 'Giao dịch đã được xác thực thành công. Đơn hàng của bạn đang được xử lý.';
            // Hoàn tất checkout và làm sạch giỏ hàng
            this.cartService.completeCheckout();
          } else {
            this.message = res.message || 'Thanh toán không thành công hoặc đã bị hủy.';
          }
          
          // Trích xuất mã đơn hàng từ vnp_OrderInfo
          const orderInfo = decodeURIComponent(params['vnp_OrderInfo'] || '');
          const match = /don hang\s+(ORD-[A-Z0-9-]+)/i.exec(orderInfo);
          if (match && match[1]) {
            this.orderCode = match[1];
          } else {
            this.orderCode = 'VNPAY-ORD';
          }
          this.cdr.detectChanges();
        },
        error: () => {
          this.isSuccess = false;
          this.message = 'Lỗi hệ thống khi xác thực kết quả giao dịch.';
          this.orderCode = 'N/A';
          this.cdr.detectChanges();
        }
      });
    });
  }

  viewOrderDetails() {
    this.router.navigate(['/user/orders']);
  }

  backToHome() {
    this.router.navigate(['/']);
  }
}
