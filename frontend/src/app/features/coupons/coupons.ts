import { Component, inject, OnInit, signal, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CouponService, CouponDto } from '../../core/services/coupon.service';
import { CartService } from '../../core/services/cart.service';
import { ToastService } from '../../core/services/toast.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-user-coupons',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './coupons.html',
  styleUrl: './coupons.css',
})
export class CouponsComponent implements OnInit {
  private couponService = inject(CouponService);
  private cartService = inject(CartService);
  private toast = inject(ToastService);
  private auth = inject(AuthService);
  private platformId = inject(PLATFORM_ID);

  myCoupons = signal<CouponDto[]>([]);
  isLoading = signal(true);

  // Nhập mã nhận coupon
  inputCode = '';
  isValidating = signal(false);
  validatedCoupon = signal<CouponDto | null>(null);

  // Trạng thái copy
  copiedCode = signal<string | null>(null);

  ngOnInit(): void {
    this.loadMyCoupons();
  }

  loadMyCoupons(): void {
    this.isLoading.set(true);

    // Bảo vệ khi render ở phía Server (Node.js / SSR)
    if (!isPlatformBrowser(this.platformId)) {
      this.myCoupons.set([]);
      this.isLoading.set(false);
      return;
    }

    try {
      const stored = localStorage.getItem('collected_coupons');
      if (stored) {
        const list = JSON.parse(stored) as CouponDto[];
        // Filter out expired coupons
        const now = new Date();
        const activeList = list.filter((c) => new Date(c.endDate) >= now);
        this.myCoupons.set(activeList);
        localStorage.setItem('collected_coupons', JSON.stringify(activeList));
      } else {
        this.myCoupons.set([]);
      }
    } catch (e) {
      console.error('Error reading collected_coupons:', e);
      this.myCoupons.set([]);
    } finally {
      this.isLoading.set(false);
    }
  }

  // Nhập mã để nhận/kiểm tra Coupon
  checkCoupon(): void {
    if (!this.inputCode.trim()) {
      this.toast.warning('Vui lòng nhập mã giảm giá');
      return;
    }

    this.isValidating.set(true);
    const code = this.inputCode.trim().toUpperCase();

    this.couponService.getByCode(code).subscribe({
      next: (coupon) => {
        this.isValidating.set(false);
        if (coupon) {
          // Check if it's active and not expired
          const now = new Date();
          const endDate = new Date(coupon.endDate);
          const startDate = new Date(coupon.startDate);

          if (!coupon.isActive || now > endDate) {
            this.toast.error('Mã giảm giá đã hết hạn hoặc không còn hoạt động');
            this.validatedCoupon.set(null);
            return;
          }
          if (now < startDate) {
            this.toast.error('Mã giảm giá này chưa đến thời gian sử dụng');
            this.validatedCoupon.set(null);
            return;
          }

          this.validatedCoupon.set(coupon);
          this.addCouponToWallet(coupon);
        }
      },
      error: (err) => {
        this.isValidating.set(false);
        const errMsg = err?.error?.message || 'Không thể kiểm tra mã lúc này';
        this.toast.error(errMsg);
        this.validatedCoupon.set(null);
      },
    });
  }

  // Thêm Coupon vào ví cá nhân và lưu trữ
  addCouponToWallet(coupon: CouponDto): void {
    const list = [...this.myCoupons()];
    const exists = list.some((c) => c.code === coupon.code);
    if (!exists) {
      list.unshift(coupon);
      this.myCoupons.set(list);
      if (isPlatformBrowser(this.platformId)) {
        localStorage.setItem('collected_coupons', JSON.stringify(list));
      }
      this.toast.success(
        `Chúc mừng! Bạn đã nhận và lưu thành công mã giảm giá [${coupon.code}] vào ví.`,
      );
    } else {
      this.toast.info(`Mã giảm giá [${coupon.code}] đã có sẵn trong ví của bạn.`);
    }
  }



  // Sao chép mã coupon
  copyCode(code: string): void {
    navigator.clipboard.writeText(code).then(() => {
      this.copiedCode.set(code);
      this.toast.success(`Đã sao chép mã: ${code}`);
      setTimeout(() => {
        if (this.copiedCode() === code) {
          this.copiedCode.set(null);
        }
      }, 2000);
    });
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
  }

  isExpired(endDateStr: string): boolean {
    if (!endDateStr) return true;
    const end = new Date(endDateStr);
    const now = new Date();
    return end < now;
  }

  getDaysLeft(endDateStr: string): number {
    const end = new Date(endDateStr);
    const now = new Date();
    const diffTime = end.getTime() - now.getTime();
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    return diffDays > 0 ? diffDays : 0;
  }
}
