import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, OnChanges, SimpleChanges, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { CouponService } from '../../../core/services/coupon.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

export interface CouponData {
  couponId?: string;
  code: string;
  description?: string;
  discountType: string;
  discountValue: number;
  minOrderAmount: number;
  maxDiscount?: number;
  usageLimit?: number;
  perUserLimit?: number;
  startDate: string | Date;
  endDate: string | Date;
  isActive?: boolean;
}

@Component({
  selector: 'app-coupon-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="modal-overlay" *ngIf="isOpen" (click)="onCancel()" role="dialog" aria-modal="true" aria-labelledby="coupon-form-title" tabindex="0" (keydown.escape)="onCancel()">
      <div class="modal-content" (click)="$event.stopPropagation()">
        <div class="modal-header">
          <h2 id="coupon-form-title">{{ isEditMode ? 'Sửa Coupon' : 'Tạo Coupon Mới' }}</h2>
          <button class="close-btn" (click)="onCancel()" aria-label="Đóng">✕</button>
        </div>
        <form [formGroup]="couponForm" (ngSubmit)="onSubmit()">
          <div class="form-group">
            <label for="code">Mã Coupon</label>
            <input id="code" type="text" formControlName="code" placeholder="VD: SUMMER20" [readonly]="isEditMode" />
            <span class="error" *ngIf="couponForm.get('code')?.errors && couponForm.get('code')?.touched">{{ getErrorMessage('code') }}</span>
          </div>
          <div class="form-group">
            <label for="description">Mô Tả</label>
            <textarea id="description" formControlName="description" placeholder="Mô tả chi tiết coupon"></textarea>
            <span class="error" *ngIf="couponForm.get('description')?.errors && couponForm.get('description')?.touched">{{ getErrorMessage('description') }}</span>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label for="discountType">Loại Giảm Giá</label>
              <select id="discountType" formControlName="discountType">
                <option value="">-- Chọn loại --</option>
                <option value="percentage">Phần Trăm (%)</option>
                <option value="fixed">Cố Định (VND)</option>
              </select>
              <span class="error" *ngIf="couponForm.get('discountType')?.errors && couponForm.get('discountType')?.touched">{{ getErrorMessage('discountType') }}</span>
            </div>
            <div class="form-group">
              <label for="discountValue">
                Mức Giảm
                <span class="hint" *ngIf="couponForm.get('discountType')?.value === 'percentage'">(1 – 100%)</span>
                <span class="hint" *ngIf="couponForm.get('discountType')?.value === 'fixed'">(VND, tối thiểu 0)</span>
              </label>
              <input id="discountValue" type="number" formControlName="discountValue"
                [placeholder]="couponForm.get('discountType')?.value === 'percentage' ? 'VD: 20' : 'VD: 50000'"
                [min]="couponForm.get('discountType')?.value === 'percentage' ? 1 : 0"
                [attr.max]="couponForm.get('discountType')?.value === 'percentage' ? 100 : null" />
              <span class="error" *ngIf="couponForm.get('discountValue')?.errors && couponForm.get('discountValue')?.touched">{{ getErrorMessage('discountValue') }}</span>
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label for="minOrderAmount">Giá Tối Thiểu (VND)</label>
              <input id="minOrderAmount" type="number" formControlName="minOrderAmount" placeholder="VD: 100000" min="0" />
              <small class="help-text">Đơn hàng phải đạt giá trị này mới áp dụng được coupon</small>
              <span class="error" *ngIf="couponForm.get('minOrderAmount')?.errors && couponForm.get('minOrderAmount')?.touched">{{ getErrorMessage('minOrderAmount') }}</span>
            </div>
            <div class="form-group">
              <label for="maxDiscount">Giảm Tối Đa (VND) <span class="hint">(tùy chọn)</span></label>
              <input id="maxDiscount" type="number" formControlName="maxDiscount" placeholder="VD: 200000" min="0" />
              <span class="error" *ngIf="couponForm.get('maxDiscount')?.errors && couponForm.get('maxDiscount')?.touched">{{ getErrorMessage('maxDiscount') }}</span>
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label for="usageLimit">Lượt Sử Dụng Tối Đa <span class="hint">(tùy chọn)</span></label>
              <input id="usageLimit" type="number" formControlName="usageLimit" placeholder="VD: 1000" min="1" />
              <span class="error" *ngIf="couponForm.get('usageLimit')?.errors && couponForm.get('usageLimit')?.touched">{{ getErrorMessage('usageLimit') }}</span>
            </div>
            <div class="form-group">
              <label for="perUserLimit">Tối Đa/Người <span class="hint">(tùy chọn)</span></label>
              <input id="perUserLimit" type="number" formControlName="perUserLimit" placeholder="VD: 2" min="1" />
              <span class="error" *ngIf="couponForm.get('perUserLimit')?.errors && couponForm.get('perUserLimit')?.touched">{{ getErrorMessage('perUserLimit') }}</span>
            </div>
          </div>
          <div class="form-row">
            <div class="form-group">
              <label for="startDate">Ngày Bắt Đầu</label>
              <input id="startDate" type="datetime-local" formControlName="startDate" [min]="minDateTime" />
              <small class="help-text">Phải từ thời điểm hiện tại trở đi</small>
              <span class="error" *ngIf="couponForm.get('startDate')?.errors && couponForm.get('startDate')?.touched">{{ getErrorMessage('startDate') }}</span>
            </div>
            <div class="form-group">
              <label for="endDate">Ngày Kết Thúc</label>
              <input id="endDate" type="datetime-local" formControlName="endDate"
                [min]="couponForm.get('startDate')?.value || minDateTime" />
              <small class="help-text">Phải lớn hơn hoặc bằng ngày bắt đầu</small>
              <span class="error" *ngIf="couponForm.get('endDate')?.errors && couponForm.get('endDate')?.touched">{{ getErrorMessage('endDate') }}</span>
            </div>
          </div>
          <div class="form-group checkbox">
            <input id="isActive" type="checkbox" formControlName="isActive" />
            <label for="isActive">Kích Hoạt</label>
          </div>
          <div class="error-message" *ngIf="apiError">{{ apiError }}</div>
          <div class="modal-footer">
            <button type="button" class="btn-cancel" (click)="onCancel()">Hủy</button>
            <button type="submit" class="btn-submit" [disabled]="loading || !couponForm.valid">
              {{ loading ? 'Đang lưu...' : (isEditMode ? 'Cập Nhật' : 'Tạo') }}
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
  styles: [
    `.modal-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,0.5); display: flex; align-items: center; justify-content: center; z-index: 1000; }`,
    `.modal-content { background: white; border-radius: 8px; width: 90%; max-width: 600px; max-height: 90vh; overflow-y: auto; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }`,
    `.modal-header { display: flex; justify-content: space-between; align-items: center; padding: 20px; border-bottom: 1px solid #e0e0e0; background: #f8f9fa; }`,
    `.modal-header h2 { margin: 0; font-size: 18px; color: #333; }`,
    `.close-btn { background: none; border: none; font-size: 24px; cursor: pointer; color: #666; }`,
    `form { padding: 20px; }`,
    `.form-group { margin-bottom: 16px; }`,
    `.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; }`,
    `label { display: block; font-weight: 600; margin-bottom: 6px; color: #333; font-size: 14px; }`,
    `.hint { font-weight: 400; color: #888; font-size: 12px; margin-left: 4px; }`,
    `input, select, textarea { width: 100%; padding: 10px; border: 1px solid #d0d0d0; border-radius: 4px; font-size: 14px; font-family: inherit; box-sizing: border-box; }`,
    `textarea { resize: vertical; min-height: 80px; }`,
    `input:focus, select:focus, textarea:focus { outline: none; border-color: #007bff; box-shadow: 0 0 0 3px rgba(0,123,255,0.1); }`,
    `input[readonly] { background: #f5f5f5; cursor: not-allowed; }`,
    `.checkbox { display: flex; align-items: center; gap: 8px; }`,
    `.checkbox input { width: auto; }`,
    `.help-text { display: block; font-size: 11px; color: #888; margin-top: 4px; }`,
    `.error { display: block; color: #dc3545; font-size: 12px; margin-top: 4px; }`,
    `.error-message { color: #dc3545; background: #f8d7da; border: 1px solid #f5c6cb; padding: 12px; border-radius: 4px; margin-bottom: 16px; font-size: 14px; }`,
    `.modal-footer { display: flex; gap: 12px; justify-content: flex-end; padding: 20px; border-top: 1px solid #e0e0e0; background: #f8f9fa; }`,
    `.btn-cancel, .btn-submit { padding: 10px 20px; border: none; border-radius: 4px; font-size: 14px; cursor: pointer; font-weight: 600; }`,
    `.btn-cancel { background: #e0e0e0; color: #333; }`,
    `.btn-cancel:hover { background: #d0d0d0; }`,
    `.btn-submit { background: #007bff; color: white; }`,
    `.btn-submit:hover:not(:disabled) { background: #0056b3; }`,
    `.btn-submit:disabled { background: #ccc; cursor: not-allowed; }`
  ]
})
export class CouponFormComponent implements OnInit, OnDestroy, OnChanges {
  @Input() isOpen = false;
  @Input() isEditMode = false;
  @Input() couponData: CouponData | null = null;
  @Output() save = new EventEmitter<unknown>();
  @Output() closeForm = new EventEmitter<void>();

  couponForm!: FormGroup;
  loading = false;
  apiError = '';

  /** Giá trị min cho datetime-local input = thời điểm hiện tại */
  minDateTime = this.toLocalDateTimeString(new Date());

  private destroy$ = new Subject<void>();
  private fb = inject(FormBuilder);
  private couponService = inject(CouponService);

  ngOnInit(): void {
    this.initForm();
  }

  ngOnChanges(changes: SimpleChanges): void {
    // Mỗi khi modal được mở → cập nhật minDateTime và reset form với dữ liệu mới
    if (changes['isOpen']?.currentValue === true && this.couponForm) {
      this.minDateTime = this.toLocalDateTimeString(new Date());
      this.apiError = '';
      this.resetFormData();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ─── Custom Validators ────────────────────────────────────────────────────────

  /** startDate phải >= thời điểm hiện tại */
  private startDateValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      const now = new Date();
      const selected = new Date(control.value);
      return selected >= now ? null : { pastDate: true };
    };
  }

  /** endDate phải >= startDate */
  private endDateAfterStartValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) return null;
      const startCtrl = this.couponForm?.get('startDate');
      if (!startCtrl?.value) return null;
      const start = new Date(startCtrl.value);
      const end = new Date(control.value);
      return end >= start ? null : { endBeforeStart: true };
    };
  }

  // ─── Form Init ────────────────────────────────────────────────────────────────

  private initForm(): void {
    this.couponForm = this.fb.group({
      code: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(20)]],
      description: ['', [Validators.maxLength(255)]],
      discountType: ['', Validators.required],
      discountValue: ['', [Validators.required, Validators.min(0)]],
      minOrderAmount: ['', [Validators.required, Validators.min(0)]],
      maxDiscount: ['', [Validators.min(0)]],
      usageLimit: ['', [Validators.min(1)]],
      perUserLimit: ['', [Validators.min(1)]],
      startDate: ['', [Validators.required, this.startDateValidator()]],
      endDate: ['', [Validators.required, this.endDateAfterStartValidator()]],
      isActive: [true]
    });

    // Cập nhật validator discountValue khi đổi loại giảm giá
    this.couponForm.get('discountType')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(type => {
        const ctrl = this.couponForm.get('discountValue');
        if (type === 'percentage') {
          ctrl?.setValidators([Validators.required, Validators.min(1), Validators.max(100)]);
        } else {
          ctrl?.setValidators([Validators.required, Validators.min(0)]);
        }
        ctrl?.updateValueAndValidity();
      });

    // Khi startDate thay đổi → re-validate endDate
    this.couponForm.get('startDate')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.couponForm.get('endDate')?.updateValueAndValidity();
      });

    this.resetFormData();
  }

  /** Reset và patch form theo isEditMode + couponData hiện tại */
  private resetFormData(): void {
    if (this.isEditMode && this.couponData) {
      this.couponForm.patchValue({
        code: this.couponData.code,
        description: this.couponData.description ?? '',
        discountType: this.couponData.discountType,
        discountValue: this.couponData.discountValue,
        minOrderAmount: this.couponData.minOrderAmount,
        maxDiscount: this.couponData.maxDiscount ?? '',
        usageLimit: this.couponData.usageLimit ?? '',
        perUserLimit: this.couponData.perUserLimit ?? '',
        startDate: this.toLocalDateTimeString(new Date(this.couponData.startDate)),
        endDate: this.toLocalDateTimeString(new Date(this.couponData.endDate)),
        isActive: this.couponData.isActive ?? true
      });
      this.couponForm.get('code')?.disable();
      // Edit mode: startDate có thể nằm trong quá khứ (coupon đã tạo)
      // → bỏ validator pastDate, chỉ giữ required
      this.couponForm.get('startDate')?.setValidators([Validators.required]);
      this.couponForm.get('startDate')?.updateValueAndValidity();
    } else {
      // Tạo mới: reset hoàn toàn, enable lại trường code
      this.couponForm.reset({ isActive: true });
      this.couponForm.get('code')?.enable();
    }
    // Đảm bảo validator discountValue đúng với loại hiện tại
    const type = this.couponForm.get('discountType')?.value;
    const ctrl = this.couponForm.get('discountValue');
    if (type === 'percentage') {
      ctrl?.setValidators([Validators.required, Validators.min(1), Validators.max(100)]);
    } else {
      ctrl?.setValidators([Validators.required, Validators.min(0)]);
    }
    ctrl?.updateValueAndValidity();
  }

  /**
   * Chuyển Date sang chuỗi "yyyy-MM-ddTHH:mm" dùng múi giờ local
   * (ISO slice sẽ bị lệch UTC so với local time)
   */
  private toLocalDateTimeString(date: Date): string {
    if (!date || isNaN(date.getTime())) return '';
    const pad = (n: number) => n.toString().padStart(2, '0');
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
  }

  // ─── Error Messages ───────────────────────────────────────────────────────────

  getErrorMessage(field: string): string {
    const ctrl = this.couponForm.get(field);
    if (!ctrl) return '';
    if (ctrl.hasError('required')) return 'Bắt buộc';
    if (ctrl.hasError('minlength')) return `Tối thiểu ${ctrl.errors?.['minlength'].requiredLength} ký tự`;
    if (ctrl.hasError('maxlength')) return `Tối đa ${ctrl.errors?.['maxlength'].requiredLength} ký tự`;
    if (ctrl.hasError('min')) {
      if (field === 'discountValue' && this.couponForm.get('discountType')?.value === 'percentage') {
        return 'Phần trăm tối thiểu là 1%';
      }
      return `Giá trị tối thiểu là ${ctrl.errors?.['min'].min}`;
    }
    if (ctrl.hasError('max')) return `Phần trăm tối đa là ${ctrl.errors?.['max'].max}%`;
    if (ctrl.hasError('pastDate')) return 'Ngày bắt đầu phải từ thời điểm hiện tại trở đi';
    if (ctrl.hasError('endBeforeStart')) return 'Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu';
    return 'Không hợp lệ';
  }

  // ─── Submit ───────────────────────────────────────────────────────────────────

  onSubmit(): void {
    if (this.couponForm.invalid) {
      this.couponForm.markAllAsTouched();
      return;
    }
    this.loading = true;
    this.apiError = '';
    const raw = this.couponForm.getRawValue();
    const payload = {
      ...raw,
      startDate: new Date(raw.startDate).toISOString(),
      endDate: new Date(raw.endDate).toISOString()
    };
    const request = this.isEditMode && this.couponData?.couponId
      ? this.couponService.update(this.couponData.couponId, payload)
      : this.couponService.create(payload);
    request.pipe(takeUntil(this.destroy$)).subscribe({
      next: result => {
        this.loading = false;
        this.save.emit(result);
        this.onCancel();
      },
      error: err => {
        this.loading = false;
        this.apiError = err?.error?.message || 'Lỗi khi lưu coupon';
      }
    });
  }

  onCancel(): void {
    this.closeForm.emit();
  }
}
