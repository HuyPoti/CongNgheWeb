import { Component, Input, Output, EventEmitter, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CouponService, CouponValidationResultDto, CouponValidationItemDto } from '../../core/services/coupon.service';
import { CartService } from '../../core/services/cart.service';

export interface AppliedCoupon {
  code: string;
  couponId: string;
  discountAmount: number;
  finalAmount: number;
}

@Component({
  selector: 'app-coupon-input',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="coupon-container">
      @if (appliedCoupon()) {
        <div class="applied-coupon">
          <div class="coupon-info">
            <span class="coupon-badge">{{ appliedCoupon()!.code }}</span>
            <div class="discount-details">
              <p class="discount-label">Mã giảm giá đã áp dụng</p>
              <p class="discount-amount">-{{ appliedCoupon()!.discountAmount | number:'1.0-0' }} VND</p>
            </div>
          </div>
          <button type="button" (click)="removeCoupon()" class="btn-remove" title="Xóa mã">
            <span>✕</span>
          </button>
        </div>
      } @else {
        <form [formGroup]="couponForm" (ngSubmit)="onApply()" class="coupon-form">
          <div class="input-group">
            <input 
              type="text" 
              formControlName="code" 
              placeholder="Nhập mã giảm giá"
              class="coupon-input"
              [disabled]="validating()"
            />
            <button 
              type="submit" 
              class="btn-apply" 
              [disabled]="!couponForm.valid || validating()"
            >
              @if (validating()) {
                <span class="loader"></span>
              }
              {{ validating() ? 'Đang áp...' : 'Áp dụng' }}
            </button>
          </div>
        </form>
      }

      @if (error()) {
        <div class="error-message">
          <span class="error-icon">⚠</span>
          {{ error() }}
        </div>
      }

      @if (success()) {
        <div class="success-message">
          <span class="success-icon">✓</span>
          {{ success() }}
        </div>
      }
    </div>
  `,
  styles: [`
    .coupon-container {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .coupon-form {
      display: flex;
      flex-direction: column;
    }

    .input-group {
      display: flex;
      gap: 8px;
    }

    .coupon-input {
      flex: 1;
      padding: 10px 14px;
      border: 1.5px solid #cbd5e1;
      border-radius: 10px;
      font-size: 14px;
      font-family: inherit;
      font-weight: 600;
      color: #1e293b;
      transition: all 0.25s ease;
      background: #f8fafc;
    }

    .coupon-input:focus {
      outline: none;
      border-color: #6366f1;
      background: white;
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
    }

    .coupon-input:disabled {
      background: #f1f5f9;
      cursor: not-allowed;
    }

    .btn-apply {
      padding: 10px 20px;
      background: linear-gradient(135deg, #4f46e5 0%, #6366f1 100%);
      color: white;
      border: none;
      border-radius: 10px;
      font-weight: 700;
      cursor: pointer;
      font-size: 14px;
      display: flex;
      align-items: center;
      gap: 6px;
      justify-content: center;
      transition: all 0.2s ease;
      white-space: nowrap;
      box-shadow: 0 4px 10px rgba(79, 70, 229, 0.15);
    }

    .btn-apply:hover:not(:disabled) {
      transform: translateY(-1px);
      box-shadow: 0 6px 14px rgba(79, 70, 229, 0.25);
    }

    .btn-apply:disabled {
      background: #cbd5e1;
      box-shadow: none;
      cursor: not-allowed;
    }

    .loader {
      display: inline-block;
      width: 12px;
      height: 12px;
      border: 2px solid #ffffff;
      border-top-color: transparent;
      border-radius: 50%;
      animation: spin 0.6s linear infinite;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    .applied-coupon {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 12px 16px;
      background: #f0fdf4;
      border: 1.5px solid #86efac;
      border-radius: 12px;
    }

    .coupon-info {
      display: flex;
      align-items: center;
      gap: 12px;
      flex: 1;
    }

    .coupon-badge {
      display: inline-block;
      padding: 4px 10px;
      background: #10b981;
      color: white;
      border-radius: 8px;
      font-weight: 700;
      font-family: monospace;
      font-size: 13px;
      white-space: nowrap;
    }

    .discount-details {
      display: flex;
      flex-direction: column;
      gap: 2px;
    }

    .discount-label {
      margin: 0;
      font-size: 11px;
      font-weight: 600;
      color: #64748b;
    }

    .discount-amount {
      margin: 0;
      font-size: 14px;
      font-weight: 750;
      color: #15803d;
    }

    .btn-remove {
      padding: 6px 10px;
      background: none;
      border: 1px solid #10b981;
      color: #10b981;
      border-radius: 8px;
      cursor: pointer;
      font-weight: 700;
      transition: all 0.2s;
    }

    .btn-remove:hover {
      background: #10b981;
      color: white;
    }

    .error-message {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px 12px;
      background: #fef2f2;
      border: 1px solid #fecaca;
      border-radius: 8px;
      color: #b91c1c;
      font-size: 13px;
      font-weight: 500;
    }

    .error-icon {
      font-size: 14px;
      font-weight: bold;
    }

    .success-message {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 8px 12px;
      background: #f0fdf4;
      border: 1px solid #bbf7d0;
      border-radius: 8px;
      color: #15803d;
      font-size: 13px;
      font-weight: 500;
    }

    .success-icon {
      font-size: 14px;
      font-weight: bold;
    }
  `]
})
export class CouponInputComponent implements OnInit {
  @Input() totalAmount = 0;
  @Input() userId?: string;
  @Input() items: CouponValidationItemDto[] = [];
  @Output() couponApplied = new EventEmitter<AppliedCoupon>();
  @Output() couponRemoved = new EventEmitter<void>();

  couponForm!: FormGroup;
  validating = signal(false);
  error = signal<string | null>(null);
  success = signal<string | null>(null);
  appliedCoupon = signal<AppliedCoupon | null>(null);

  private fb = inject(FormBuilder);
  private couponService = inject(CouponService);
  private cartService = inject(CartService);

  ngOnInit() {
    this.initForm();
    this.autoApplySavedCoupon();
  }

  private initForm() {
    this.couponForm = this.fb.group({
      code: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(20)]]
    });
  }

  private autoApplySavedCoupon() {
    const savedCode = this.cartService.appliedCoupon();
    if (savedCode) {
      this.applyCode(savedCode);
    }
  }

  onApply() {
    if (!this.couponForm.valid) return;
    const code = this.couponForm.get('code')?.value?.trim();
    if (!code) return;
    this.applyCode(code);
  }

  private applyCode(code: string) {
    this.validating.set(true);
    this.error.set(null);
    this.success.set(null);

    this.couponService.validate({
      code,
      totalAmount: this.totalAmount,
      userId: this.userId,
      items: this.items
    }).subscribe({
      next: (result: CouponValidationResultDto) => {
        this.validating.set(false);
        
        if (result.isValid && result.couponId) {
          const applied: AppliedCoupon = {
            code: result.code || code,
            couponId: result.couponId,
            discountAmount: result.discountAmount,
            finalAmount: result.finalAmount
          };
          this.appliedCoupon.set(applied);
          this.success.set('Mã giảm giá đã áp dụng thành công!');
          this.couponForm.reset();
          this.cartService.setAppliedCoupon(applied.code);
          this.couponApplied.emit(applied);
        } else {
          this.error.set(result.message || 'Mã không hợp lệ');
          this.cartService.setAppliedCoupon(null);
        }
      },
      error: (err) => {
        this.validating.set(false);
        this.error.set(err?.error?.message || 'Lỗi kiểm tra mã');
        this.cartService.setAppliedCoupon(null);
      }
    });
  }

  removeCoupon() {
    this.appliedCoupon.set(null);
    this.error.set(null);
    this.success.set(null);
    this.couponForm.reset();
    this.cartService.setAppliedCoupon(null);
    this.couponRemoved.emit();
  }
}
