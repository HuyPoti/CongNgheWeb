import { Component, inject, OnInit, signal, effect } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { CartService } from '../../../core/services/cart.service';
import { CouponService, CouponDto } from '../../../core/services/coupon.service';

@Component({
  selector: 'app-cart-page',
  standalone: true,
  imports: [RouterLink, CommonModule, TranslatePipe, FormsModule],
  templateUrl: './cart-page.html',
  styleUrl: './cart-page.css',
})
export class CartPage implements OnInit {
  private readonly cartService = inject(CartService);
  private readonly couponService = inject(CouponService);
  private readonly router = inject(Router);

  readonly cartItems = this.cartService.getCartItems;
  readonly subtotal = this.cartService.subtotal;

  nudgeCoupon = signal<CouponDto | null>(null);
  nudgeAmountNeeded = signal<number>(0);

  constructor() {
    effect(() => {
      // Auto recalculate whenever the cart subtotal changes
      this.calculateNudge();
    });
  }

  ngOnInit() {
    this.calculateNudge();
  }

  calculateNudge() {
    const currentSubtotal = this.subtotal();
    if (currentSubtotal === 0) {
      this.nudgeCoupon.set(null);
      this.nudgeAmountNeeded.set(0);
      return;
    }

    this.couponService.getActiveCoupons().subscribe({
      next: (res) => {
        const coupons = res.items || [];
        
        // Find coupons that have higher minimum order amount but are close to current subtotal
        const possibleNudges = coupons
          .filter(c => c.isActive && c.minOrderAmount > currentSubtotal)
          .map(c => ({
            coupon: c,
            needed: c.minOrderAmount - currentSubtotal
          }))
          // Only show nudge if amount needed is reasonable (<= 2,000,000đ or <= 40% of minOrderAmount)
          .filter(item => item.needed <= 2000000 || item.needed <= item.coupon.minOrderAmount * 0.4)
          .sort((a, b) => a.needed - b.needed);

        if (possibleNudges.length > 0) {
          this.nudgeCoupon.set(possibleNudges[0].coupon);
          this.nudgeAmountNeeded.set(possibleNudges[0].needed);
        } else {
          this.nudgeCoupon.set(null);
          this.nudgeAmountNeeded.set(0);
        }
      },
      error: () => {
        this.nudgeCoupon.set(null);
        this.nudgeAmountNeeded.set(0);
      }
    });
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

  proceedToCheckout() {
    void this.router.navigate(['/cart/checkout']);
  }
}
