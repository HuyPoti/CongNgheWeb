import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CartService } from '../../../core/services/cart.service';
import { OrderService } from '../../../core/services/order.service';
import { PaymentService, PaymentResponse } from '../../../core/services/payment.service';
import { ToastService } from '../../../core/services/toast.service';

interface ShippingAddress {
  recipientName: string;
  phone: string;
  addressLine: string;
  province?: string;
  district?: string;
  ward?: string;
}

@Component({
  selector: 'app-payment',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './payment.html',
  styles: ``,
})
export class Payment implements OnInit {
  private router = inject(Router);
  private orderService = inject(OrderService);
  private paymentService = inject(PaymentService);
  private toastService = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);
  cartService = inject(CartService);

  shippingAddress: ShippingAddress | null = null;
  paymentMethod: 'cod' | 'bank_transfer' | 'vnpay' = 'cod';
  isPlacingOrder = false;

  paymentResult: PaymentResponse | null = null;
  showBankInfo = false;
  orderId = '';
  orderCode = '';

  constructor() {
    if (typeof window !== 'undefined') {
      const state = history.state as { shippingAddress?: ShippingAddress };
      if (state && state.shippingAddress) {
        this.shippingAddress = state.shippingAddress;
      }
    }
  }

  ngOnInit() {
    if (!this.shippingAddress || this.cartService.getCartItems().length === 0) {
      void this.router.navigate(['/cart/checkout']);
    }
  }

  placeOrder() {
    if (this.isPlacingOrder || !this.shippingAddress) return;

    this.isPlacingOrder = true;

    const payload = {
      paymentMethod: this.paymentMethod,
      shippingAddress: this.shippingAddress,
      shippingFee: 30000,
      items: this.cartService.getCartItems().map((item) => ({
        productId: item.id,
        quantity: item.quantity,
      })),
      notes: 'Created from web checkout flow'
    };

    this.orderService.create(payload).subscribe({
      next: (order) => {
        this.orderId = order.orderId;
        this.orderCode = order.orderCode;

        const paymentRequest = {
          orderId: order.orderId,
          paymentMethod: this.paymentMethod,
          returnUrl: this.paymentMethod === 'vnpay'
            ? window.location.origin + '/cart/vnpay-return'
            : undefined,
        };

        this.paymentService.create(paymentRequest).subscribe({
          next: (payment) => {
            this.paymentResult = payment;
            this.isPlacingOrder = false;

            if (this.paymentMethod === 'bank_transfer') {
              this.showBankInfo = true;
              this.toastService.success('Đơn hàng đã tạo! Vui lòng chuyển khoản theo thông tin bên dưới.');
              this.cdr.detectChanges();
            } else if (this.paymentMethod === 'cod') {
              this.toastService.success(`Đặt hàng thành công! Mã đơn: ${order.orderCode}`);
              this.cartService.clearCart();
              void this.router.navigate(['/']);
            } else if (this.paymentMethod === 'vnpay' && payment.paymentUrl) {
              window.location.href = payment.paymentUrl;
            }
          },
          error: () => {
            this.isPlacingOrder = false;
            this.toastService.error('Tạo thanh toán thất bại');
            this.cdr.detectChanges();
          },
        });
      },
      error: (err: unknown) => {
        this.isPlacingOrder = false;
        const errorObj = err as { error?: { message?: string } };
        const message = errorObj?.error?.message ?? 'Đặt hàng thất bại';
        this.toastService.error(message);
        this.cdr.detectChanges();
      },
    });
  }

  goHome() {
    this.cartService.clearCart();
    void this.router.navigate(['/']);
  }
}
