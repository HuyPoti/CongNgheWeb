import { Component, OnInit, OnDestroy, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { FlashSaleService } from '../../core/services/flash-sale.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-flash-sale-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="modal-overlay" *ngIf="isOpen" (click)="onCancel()">
      <div class="modal-content" (click)="$event.stopPropagation()">
        <div class="modal-header">
          <h2>{{ isEditMode ? 'Sửa Flash Sale' : 'Tạo Flash Sale Mới' }}</h2>
          <button class="close-btn" (click)="onCancel()">✕</button>
        </div>

        <form [formGroup]="flashSaleForm" (ngSubmit)="onSubmit()">
          <div class="form-group">
            <label>Tên Flash Sale</label>
            <input type="text" formControlName="title" placeholder="VD: Flash Sale Hè 2026" />
            <span class="error" *ngIf="flashSaleForm.get('title')?.errors && flashSaleForm.get('title')?.touched">
              {{ getErrorMessage('title') }}
            </span>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Thời Gian Bắt Đầu</label>
              <input type="datetime-local" formControlName="startTime" />
              <span class="error" *ngIf="flashSaleForm.get('startTime')?.errors && flashSaleForm.get('startTime')?.touched">
                {{ getErrorMessage('startTime') }}
              </span>
            </div>

            <div class="form-group">
              <label>Thời Gian Kết Thúc</label>
              <input type="datetime-local" formControlName="endTime" />
              <span class="error" *ngIf="flashSaleForm.get('endTime')?.errors && flashSaleForm.get('endTime')?.touched">
                {{ getErrorMessage('endTime') }}
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
            <button type="submit" class="btn-submit" [disabled]="loading || !flashSaleForm.valid">
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
      max-width: 500px;
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

    input, select {
      width: 100%;
      padding: 10px;
      border: 1px solid #d0d0d0;
      border-radius: 4px;
      font-size: 14px;
      font-family: inherit;
    }

    input:focus, select:focus {
      outline: none;
      border-color: #007bff;
      box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.1);
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
export class FlashSaleFormComponent implements OnInit, OnDestroy {
  @Input() isOpen = false;
  @Input() isEditMode = false;
  @Input() flashSaleData: any;
  @Output() save = new EventEmitter<any>();
  @Output() close = new EventEmitter<void>();

  flashSaleForm!: FormGroup;
  loading = false;
  apiError = '';
  private destroy$ = new Subject<void>();

  private fb = inject(FormBuilder);
  private flashSaleService = inject(FlashSaleService);

  ngOnInit() {
    this.initForm();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initForm() {
    this.flashSaleForm = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      startTime: ['', Validators.required],
      endTime: ['', Validators.required],
      isActive: [true]
    });

    if (this.isEditMode && this.flashSaleData) {
      this.flashSaleForm.patchValue({
        title: this.flashSaleData.title,
        startTime: this.formatDateForInput(this.flashSaleData.startTime),
        endTime: this.formatDateForInput(this.flashSaleData.endTime),
        isActive: this.flashSaleData.isActive
      });
    }
  }

  private formatDateForInput(date: string | Date): string {
    if (!date) return '';
    const d = new Date(date);
    return d.toISOString().slice(0, 16);
  }

  getErrorMessage(fieldName: string): string {
    const field = this.flashSaleForm.get(fieldName);
    if (field?.hasError('required')) return 'Bắt buộc';
    if (field?.hasError('minLength')) return `Tối thiểu ${field.errors?.['minLength'].requiredLength} ký tự`;
    if (field?.hasError('maxLength')) return `Tối đa ${field.errors?.['maxLength'].requiredLength} ký tự`;
    return 'Không hợp lệ';
  }

  onSubmit() {
    if (!this.flashSaleForm.valid) return;

    this.loading = true;
    this.apiError = '';

    const formValue = this.flashSaleForm.value;
    const payload = {
      ...formValue,
      startTime: new Date(formValue.startTime).toISOString(),
      endTime: new Date(formValue.endTime).toISOString()
    };

    const request = this.isEditMode
      ? this.flashSaleService.update(this.flashSaleData.flashSaleId, payload)
      : this.flashSaleService.create(payload);

    request.pipe(takeUntil(this.destroy$)).subscribe({
      next: (result) => {
        this.loading = false;
        this.save.emit(result);
        this.onCancel();
      },
      error: (error) => {
        this.loading = false;
        this.apiError = error?.error?.message || 'Lỗi khi lưu flash sale';
      }
    });
  }

  onCancel() {
    this.close.emit();
  }
}
