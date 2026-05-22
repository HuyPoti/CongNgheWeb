import { Component, inject, OnInit, signal, effect } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { CartService } from '../../../core/services/cart.service';
import { CouponService, CouponDto } from '../../../core/services/coupon.service';
import { FlashSaleService } from '../../../core/services/flash-sale.service';
import { ToastService } from '../../../core/services/toast.service';

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
  private readonly flashSaleService = inject(FlashSaleService);
  private readonly toastService = inject(ToastService);
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
    // Re-validate giá cart trước khi hiển thị – đảm bảo flash sale / giá luôn đúng
    this.validateCartPrices();
    this.calculateNudge();
  }

  /** Kiểm tra và đồng bộ giá flash sale với dữ liệu mới nhất từ API */
  private validateCartPrices(): void {
    if (this.cartItems().length === 0) return;

    this.flashSaleService.getActive().pipe(
      catchError(() => of(null))
    ).subscribe(flashSale => {
      const flashItems = flashSale?.items ?? [];
      const result = this.cartService.syncFlashSalePrices(flashItems);

      if (result.changed) {
        // Hiển thị tổng hợp 1 toast thay vì nhiều cái chồng nhau
        const summary = result.messages.length === 1
          ? result.messages[0]
          : `Đã cập nhật giá cho ${result.messages.length} sản phẩm trong giỏ hàng.`;
        this.toastService.info(summary);
      }
    });
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

  getFlashSaleDiscount() {
    return this.cartItems().reduce((acc, item) => {
      const discount = Math.max(item.regularPrice - item.price, 0) * item.quantity;
      return acc + discount;
    }, 0);
  }

  getOriginalSubtotal() {
    return this.cartItems().reduce((acc, item) => acc + item.regularPrice * item.quantity, 0);
  }
}
