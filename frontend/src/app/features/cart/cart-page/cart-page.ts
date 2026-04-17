import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { CartService } from '../../../core/services/cart.service';
import { OrderService } from '../../../core/services/order.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-cart-page',
  standalone: true,
  imports: [RouterLink, CommonModule, TranslatePipe, FormsModule],
  templateUrl: './cart-page.html',
  styleUrl: './cart-page.css',
})
export class CartPage {
  private readonly cartService = inject(CartService);
  private readonly orderService = inject(OrderService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  readonly isCheckoutVisible = signal(false);
  readonly currentStep = signal(1);
  readonly isPlacingOrder = signal(false);

  firstName = '';
  lastName = '';
  phone = '';
  addressLine = '';

  cardNumber = '';
  expiry = '';
  cvv = '';

  paymentMethod = 'card';

  readonly cartItems = this.cartService.getCartItems;
  readonly subtotal = this.cartService.subtotal;

  readonly canProceedStep1 = computed(() => {
    return (
      this.firstName.trim().length > 0 &&
      this.lastName.trim().length > 0 &&
      this.phone.trim().length > 0 &&
      this.addressLine.trim().length > 0
    );
  });

  readonly canProceedStep2 = computed(() => {
    return (
      this.cardNumber.replace(/\s+/g, '').length >= 12 &&
      this.expiry.trim().length >= 4 &&
      this.cvv.trim().length >= 3
    );
  });

  readonly canDeployOrder = computed(() => {
    return this.currentStep() === 3 && this.cartItems().length > 0 && !this.isPlacingOrder();
  });

  toggleCheckout() {
    this.isCheckoutVisible.set(!this.isCheckoutVisible());
  }

  nextStep() {
    if (this.currentStep() < 3) {
      this.currentStep.update(s => s + 1);
    }
  }

  setStep(step: number) {
    if (step < this.currentStep()) {
       this.currentStep.set(step);
    }
  }

  updateQuantity(id: string, delta: number) {
    this.cartService.updateQuantity(id, delta);
  }

  removeItem(id: string) {
    this.cartService.removeFromCart(id);
  }

  clearCart() {
    this.cartService.clearCart();
  }

  proceedStep1() {
    if (!this.canProceedStep1()) {
      this.toastService.warning('Vui long nhap day du thong tin nguoi nhan');
      return;
    }
    this.nextStep();
  }

  proceedStep2() {
    if (!this.canProceedStep2()) {
      this.toastService.warning('Thong tin thanh toan chua hop le');
      return;
    }
    this.nextStep();
  }

  placeOrder() {
    if (!this.canDeployOrder()) return;

    this.isPlacingOrder.set(true);

    const payload = {
      paymentMethod: this.paymentMethod,
      shippingAddress: {
        recipientName: `${this.firstName} ${this.lastName}`.trim(),
        phone: this.phone.trim(),
        addressLine: this.addressLine.trim(),
      },
      items: this.cartItems().map((item) => ({
        productId: item.id,
        quantity: item.quantity,
      })),
      notes: 'Created from web checkout flow',
    };

    this.orderService.create(payload).subscribe({
      next: (order) => {
        this.isPlacingOrder.set(false);
        this.toastService.success(`Dat hang thanh cong. Ma don: ${order.orderCode}`);
        this.cartService.clearCart();
        this.isCheckoutVisible.set(false);
        this.currentStep.set(1);
        void this.router.navigateByUrl('/');
      },
      error: (err: unknown) => {
        this.isPlacingOrder.set(false);
        const message =
          typeof err === 'object' &&
          err !== null &&
          'error' in err &&
          typeof (err as { error?: { message?: string } }).error?.message === 'string'
            ? (err as { error: { message: string } }).error.message
            : 'Dat hang that bai';
        this.toastService.error(message);
      },
    });
  }
}
