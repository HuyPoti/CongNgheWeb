import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CartService } from '../../../core/services/cart.service';
import { OrderService } from '../../../core/services/order.service';
import { ToastService } from '../../../core/services/toast.service';

interface ShippingAddress {
  recipientName: string;
  phone: string;
  addressLine: string;
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
  private toastService = inject(ToastService);
  cartService = inject(CartService);

  shippingAddress: ShippingAddress | null = null;
  paymentMethod = 'cod';
  isPlacingOrder = false;

  constructor() {
    const navigation = this.router.getCurrentNavigation();
    const state = navigation?.extras.state as { shippingAddress: ShippingAddress } | undefined;
    if (state && state.shippingAddress) {
      this.shippingAddress = state.shippingAddress;
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
      items: this.cartService.getCartItems().map((item) => ({
        productId: item.id,
        quantity: item.quantity,
      })),
      notes: 'Created from web checkout flow',
    };

    this.orderService.create(payload).subscribe({
      next: (order) => {
        this.isPlacingOrder = false;
        this.toastService.success(`Đặt hàng thành công! Mã đơn: ${order.orderCode}`);
        this.cartService.clearCart();
        void this.router.navigate(['/']);
      },
      error: (err: unknown) => {
        this.isPlacingOrder = false;
        const errorObj = err as { error?: { message?: string } };
        const message = errorObj?.error?.message ?? 'Đặt hàng thất bại';
        this.toastService.error(message);
      },
    });
  }
}
