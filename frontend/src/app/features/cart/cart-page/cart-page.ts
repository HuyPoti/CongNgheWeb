import { Component, signal, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { CartService } from '../../../core/services/cart.service';

@Component({
  selector: 'app-cart-page',
  standalone: true,
  imports: [RouterLink, CommonModule, TranslatePipe],
  templateUrl: './cart-page.html',
  styleUrl: './cart-page.css',
})
export class CartPage {
  cartService = inject(CartService);
  isCheckoutVisible = signal(false);
  currentStep = signal(1);

  cartItems = this.cartService.getCartItems;
  subtotal = this.cartService.subtotal;

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
}
