import { Component, Input, Output, EventEmitter, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CouponService, CouponValidationResultDto } from '../../core/services/coupon.service';

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
              {{ validating() ? 'Đang kiểm...' : 'Áp dụng' }}
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
      padding: 12px;
      border: 1px solid #d0d0d0;
      border-radius: 6px;
      font-size: 14px;
      font-family: inherit;
      transition: border-color 0.3s;
    }

    .coupon-input:focus {
      outline: none;
      border-color: #007bff;
      box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.1);
    }

    .coupon-input:disabled {
      background: #f5f5f5;
      cursor: not-allowed;
    }

    .btn-apply {
      padding: 12px 24px;
      background: #007bff;
      color: white;
      border: none;
      border-radius: 6px;
      font-weight: 600;
      cursor: pointer;
      font-size: 14px;
      display: flex;
      align-items: center;
      gap: 6px;
      justify-content: center;
      transition: background 0.3s;
      white-space: nowrap;
    }

    .btn-apply:hover:not(:disabled) {
      background: #0056b3;
    }

    .btn-apply:disabled {
      background: #ccc;
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
      padding: 12px;
      background: #e8f5e9;
      border: 2px solid #4caf50;
      border-radius: 6px;
    }

    .coupon-info {
      display: flex;
      align-items: center;
      gap: 12px;
      flex: 1;
    }

    .coupon-badge {
      display: inline-block;
      padding: 6px 12px;
      background: #4caf50;
      color: white;
      border-radius: 4px;
      font-weight: 600;
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
      font-size: 12px;
      color: #666;
    }

    .discount-amount {
      margin: 0;
      font-size: 14px;
      font-weight: 600;
      color: #2e7d32;
    }

    .btn-remove {
      padding: 6px 10px;
      background: none;
      border: 1px solid #4caf50;
      color: #4caf50;
      border-radius: 4px;
      cursor: pointer;
      font-weight: 600;
      transition: all 0.3s;
    }

    .btn-remove:hover {
      background: #4caf50;
      color: white;
    }

    .error-message {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 10px 12px;
      background: #ffebee;
      border: 1px solid #ef5350;
      border-radius: 4px;
      color: #c62828;
      font-size: 13px;
    }

    .error-icon {
      font-size: 16px;
      font-weight: bold;
    }

    .success-message {
      display: flex;
      align-items: center;
      gap: 8px;
      padding: 10px 12px;
      background: #f1f8e9;
      border: 1px solid #558b2f;
      border-radius: 4px;
      color: #33691e;
      font-size: 13px;
    }

    .success-icon {
      font-size: 16px;
      font-weight: bold;
    }
  `]
})
export class CouponInputComponent implements OnInit {
  @Input() totalAmount = 0;
  @Input() userId?: string;
  @Output() couponApplied = new EventEmitter<AppliedCoupon>();
  @Output() couponRemoved = new EventEmitter<void>();

  couponForm!: FormGroup;
  validating = signal(false);
  error = signal<string | null>(null);
  success = signal<string | null>(null);
  appliedCoupon = signal<AppliedCoupon | null>(null);

  private fb = inject(FormBuilder);
  private couponService = inject(CouponService);

  ngOnInit() {
    this.initForm();
  }

  private initForm() {
    this.couponForm = this.fb.group({
      code: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(20)]]
    });
  }

  onApply() {
    if (!this.couponForm.valid) return;

    this.validating.set(true);
    this.error.set(null);
    this.success.set(null);

    const code = this.couponForm.get('code')?.value?.trim();
    if (!code) return;

    this.couponService.validate({
      code,
      totalAmount: this.totalAmount,
      userId: this.userId
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
          this.couponApplied.emit(applied);
        } else {
          this.error.set(result.message || 'Mã không hợp lệ');
        }
      },
      error: (err) => {
        this.validating.set(false);
        this.error.set(err?.error?.message || 'Lỗi kiểm tra mã');
      }
    });
  }

  removeCoupon() {
    this.appliedCoupon.set(null);
    this.error.set(null);
    this.success.set(null);
    this.couponForm.reset();
    this.couponRemoved.emit();
  }
}
