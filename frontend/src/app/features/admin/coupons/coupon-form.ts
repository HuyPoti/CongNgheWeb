import { Component, OnInit, OnDestroy, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CouponService } from '../../core/services/coupon.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-coupon-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="modal-overlay" *ngIf="isOpen" (click)="onCancel()">
      <div class="modal-content" (click)="$event.stopPropagation()">
        <div class="modal-header">
          <h2>{{ isEditMode ? 'Sửa Coupon' : 'Tạo Coupon Mới' }}</h2>
          <button class="close-btn" (click)="onCancel()">✕</button>
        </div>

        <form [formGroup]="couponForm" (ngSubmit)="onSubmit()">
          <div class="form-group">
            <label>Mã Coupon</label>
            <input type="text" formControlName="code" placeholder="VD: SUMMER20" 
              [readonly]="isEditMode" />
            <span class="error" *ngIf="couponForm.get('code')?.errors && couponForm.get('code')?.touched">
              {{ getErrorMessage('code') }}
            </span>
          </div>

          <div class="form-group">
            <label>Mô Tả</label>
            <textarea formControlName="description" placeholder="Mô tả chi tiết coupon"></textarea>
            <span class="error" *ngIf="couponForm.get('description')?.errors && couponForm.get('description')?.touched">
              {{ getErrorMessage('description') }}
            </span>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Loại Giảm Giá</label>
              <select formControlName="discountType">
                <option value="">-- Chọn loại --</option>
                <option value="PERCENTAGE">Phần Trăm (%)</option>
                <option value="FIXED">Cố Định (VND)</option>
              </select>
              <span class="error" *ngIf="couponForm.get('discountType')?.errors && couponForm.get('discountType')?.touched">
                {{ getErrorMessage('discountType') }}
              </span>
            </div>

            <div class="form-group">
              <label>Mức Giảm</label>
              <input type="number" formControlName="discountValue" placeholder="VD: 20" />
              <span class="error" *ngIf="couponForm.get('discountValue')?.errors && couponForm.get('discountValue')?.touched">
                {{ getErrorMessage('discountValue') }}
              </span>
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Giá Tối Thiểu</label>
              <input type="number" formControlName="minOrderAmount" placeholder="VD: 100000" />
              <span class="error" *ngIf="couponForm.get('minOrderAmount')?.errors && couponForm.get('minOrderAmount')?.touched">
                {{ getErrorMessage('minOrderAmount') }}
              </span>
            </div>

            <div class="form-group">
              <label>Giảm Tối Đa</label>
              <input type="number" formControlName="maxDiscount" placeholder="VD: 200000" />
              <span class="error" *ngIf="couponForm.get('maxDiscount')?.errors && couponForm.get('maxDiscount')?.touched">
                {{ getErrorMessage('maxDiscount') }}
              </span>
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Lượt Sử Dụng Tối Đa</label>
              <input type="number" formControlName="usageLimit" placeholder="VD: 1000" />
              <span class="error" *ngIf="couponForm.get('usageLimit')?.errors && couponForm.get('usageLimit')?.touched">
                {{ getErrorMessage('usageLimit') }}
              </span>
            </div>

            <div class="form-group">
              <label>Tối Đa/Người</label>
              <input type="number" formControlName="perUserLimit" placeholder="VD: 2" />
              <span class="error" *ngIf="couponForm.get('perUserLimit')?.errors && couponForm.get('perUserLimit')?.touched">
                {{ getErrorMessage('perUserLimit') }}
              </span>
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Ngày Bắt Đầu</label>
              <input type="datetime-local" formControlName="startDate" />
              <span class="error" *ngIf="couponForm.get('startDate')?.errors && couponForm.get('startDate')?.touched">
                {{ getErrorMessage('startDate') }}
              </span>
            </div>

            <div class="form-group">
              <label>Ngày Kết Thúc</label>
              <input type="datetime-local" formControlName="endDate" />
              <span class="error" *ngIf="couponForm.get('endDate')?.errors && couponForm.get('endDate')?.touched">
                {{ getErrorMessage('endDate') }}
              </span>
            </div>
          </div>

          <div class="form-group checkbox">
            <input type="checkbox" formControlName="isActive" />
            <label>Kích Hoạt</label>
          </div>

          <div class="error-message" *ngIf="apiError">
            {{ apiError }}
          </div>

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
  styles: [`
    .modal-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
    }

    .modal-content {
      background: white;
      border-radius: 8px;
      width: 90%;
      max-width: 600px;
      max-height: 90vh;
      overflow-y: auto;
      box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
    }

    .modal-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 20px;
      border-bottom: 1px solid #e0e0e0;
      background: #f8f9fa;
    }

    .modal-header h2 {
      margin: 0;
      font-size: 18px;
      color: #333;
    }

    .close-btn {
      background: none;
      border: none;
      font-size: 24px;
      cursor: pointer;
      color: #666;
    }

    form {
      padding: 20px;
    }

    .form-group {
      margin-bottom: 16px;
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 16px;
    }

    label {
      display: block;
      font-weight: 600;
      margin-bottom: 6px;
      color: #333;
      font-size: 14px;
    }

    input, select, textarea {
      width: 100%;
      padding: 10px;
      border: 1px solid #d0d0d0;
      border-radius: 4px;
      font-size: 14px;
      font-family: inherit;
    }

    textarea {
      resize: vertical;
      min-height: 80px;
    }

    input:focus, select:focus, textarea:focus {
      outline: none;
      border-color: #007bff;
      box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.1);
    }

    input[readonly] {
      background: #f5f5f5;
      cursor: not-allowed;
    }

    .checkbox {
      display: flex;
      align-items: center;
      gap: 8px;
    }

    .checkbox input {
      width: auto;
      margin: 0;
    }

    .checkbox label {
      margin: 0;
    }

    .error {
      color: #dc3545;
      font-size: 12px;
      margin-top: 4px;
    }

    .error-message {
      color: #dc3545;
      background: #f8d7da;
      border: 1px solid #f5c6cb;
      padding: 12px;
      border-radius: 4px;
      margin-bottom: 16px;
      font-size: 14px;
    }

    .modal-footer {
      display: flex;
      gap: 12px;
      justify-content: flex-end;
      padding: 20px;
      border-top: 1px solid #e0e0e0;
      background: #f8f9fa;
    }

    .btn-cancel, .btn-submit {
      padding: 10px 20px;
      border: none;
      border-radius: 4px;
      font-size: 14px;
      cursor: pointer;
      font-weight: 600;
    }

    .btn-cancel {
      background: #e0e0e0;
      color: #333;
    }

    .btn-cancel:hover {
      background: #d0d0d0;
    }

    .btn-submit {
      background: #007bff;
      color: white;
    }

    .btn-submit:hover:not(:disabled) {
      background: #0056b3;
    }

    .btn-submit:disabled {
      background: #ccc;
      cursor: not-allowed;
    }
  `]
})
export class CouponFormComponent implements OnInit, OnDestroy {
  @Input() isOpen = false;
  @Input() isEditMode = false;
  @Input() couponData: any;
  @Output() save = new EventEmitter<any>();
  @Output() close = new EventEmitter<void>();

  couponForm!: FormGroup;
  loading = false;
  apiError = '';
  private destroy$ = new Subject<void>();

  private fb = inject(FormBuilder);
  private couponService = inject(CouponService);

  ngOnInit() {
    this.initForm();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initForm() {
    this.couponForm = this.fb.group({
      code: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(20)]],
      description: ['', [Validators.maxLength(255)]],
      discountType: ['', Validators.required],
      discountValue: ['', [Validators.required, Validators.min(0)]],
      minOrderAmount: ['', [Validators.required, Validators.min(0)]],
      maxDiscount: ['', [Validators.required, Validators.min(0)]],
      usageLimit: ['', [Validators.required, Validators.min(1)]],
      perUserLimit: ['', [Validators.required, Validators.min(1)]],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      isActive: [true]
    });

    if (this.isEditMode && this.couponData) {
      this.couponForm.patchValue({
        code: this.couponData.code,
        description: this.couponData.description,
        discountType: this.couponData.discountType,
        discountValue: this.couponData.discountValue,
        minOrderAmount: this.couponData.minOrderAmount,
        maxDiscount: this.couponData.maxDiscount,
        usageLimit: this.couponData.usageLimit,
        perUserLimit: this.couponData.perUserLimit,
        startDate: this.formatDateForInput(this.couponData.startDate),
        endDate: this.formatDateForInput(this.couponData.endDate),
        isActive: this.couponData.isActive
      });
      this.couponForm.get('code')?.disable();
    }
  }

  private formatDateForInput(date: string | Date): string {
    if (!date) return '';
    const d = new Date(date);
    return d.toISOString().slice(0, 16);
  }

  getErrorMessage(fieldName: string): string {
    const field = this.couponForm.get(fieldName);
    if (field?.hasError('required')) return 'Bắt buộc';
    if (field?.hasError('minLength')) return `Tối thiểu ${field.errors?.['minLength'].requiredLength} ký tự`;
    if (field?.hasError('maxLength')) return `Tối đa ${field.errors?.['maxLength'].requiredLength} ký tự`;
    if (field?.hasError('min')) return `Tối thiểu ${field.errors?.['min'].min}`;
    return 'Không hợp lệ';
  }

  onSubmit() {
    if (!this.couponForm.valid) return;

    this.loading = true;
    this.apiError = '';

    const formValue = this.couponForm.getRawValue();
    const payload = {
      ...formValue,
      startDate: new Date(formValue.startDate).toISOString(),
      endDate: new Date(formValue.endDate).toISOString()
    };

    const request = this.isEditMode
      ? this.couponService.update(this.couponData.couponId, payload)
      : this.couponService.create(payload);

    request.pipe(takeUntil(this.destroy$)).subscribe({
      next: (result) => {
        this.loading = false;
        this.save.emit(result);
        this.onCancel();
      },
      error: (error) => {
        this.loading = false;
        this.apiError = error?.error?.message || 'Lỗi khi lưu coupon';
      }
    });
  }

  onCancel() {
    this.close.emit();
  }
}
