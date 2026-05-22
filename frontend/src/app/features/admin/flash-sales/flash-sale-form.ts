import { Component, EventEmitter, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { finalize, takeUntil } from 'rxjs/operators';
import { FlashSaleDto, FlashSaleService } from '../../../core/services/flash-sale.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-flash-sale-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="modal-overlay" *ngIf="isOpen" (click)="onCancel()">
      <div class="modal-content" (click)="$event.stopPropagation()">
        <div class="modal-header">
          <h2>{{ isEditMode ? 'Sua Flash Sale' : 'Tao Flash Sale Moi' }}</h2>
          <button class="close-btn" type="button" (click)="onCancel()">x</button>
        </div>

        <form [formGroup]="flashSaleForm" (ngSubmit)="onSubmit()">
          <div class="form-group">
            <label for="flash-sale-title">Ten Flash Sale</label>
            <input id="flash-sale-title" type="text" formControlName="title" placeholder="VD: Flash Sale He 2026" />
            <span class="error" *ngIf="shouldShowFieldError('title')">
              {{ getErrorMessage('title') }}
            </span>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label for="flash-sale-start-time">Thoi Gian Bat Dau</label>
              <input id="flash-sale-start-time" type="datetime-local" formControlName="startTime" />
              <span class="error" *ngIf="shouldShowFieldError('startTime') || flashSaleForm.hasError('startInPast')">
                {{ getErrorMessage('startTime') }}
              </span>
            </div>

            <div class="form-group">
              <label for="flash-sale-end-time">Thoi Gian Ket Thuc</label>
              <input id="flash-sale-end-time" type="datetime-local" formControlName="endTime" />
              <span class="error" *ngIf="shouldShowFieldError('endTime') || flashSaleForm.hasError('invalidRange')">
                {{ getErrorMessage('endTime') }}
              </span>
            </div>
          </div>

          <div class="form-group checkbox">
            <input id="flash-sale-active" type="checkbox" formControlName="isActive" />
            <label for="flash-sale-active">Kich Hoat</label>
          </div>

          <div class="hint">
            Flash sale dang bat buoc co thoi gian ket thuc sau thoi gian bat dau. Khi tao moi, thoi gian bat dau khong duoc nam trong qua khu.
          </div>

          <div class="modal-footer">
            <button type="button" class="btn-cancel" (click)="onCancel()">Huy</button>
            <button type="submit" class="btn-submit" [disabled]="loading() || flashSaleForm.invalid">
              {{ loading() ? 'Dang luu...' : (isEditMode ? 'Cap Nhat' : 'Tao') }}
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
      max-width: 560px;
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
      font-size: 20px;
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

    input {
      width: 100%;
      padding: 10px;
      border: 1px solid #d0d0d0;
      border-radius: 4px;
      font-size: 14px;
      font-family: inherit;
    }

    input:focus {
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

    .hint {
      color: #64748b;
      background: #f8fafc;
      border: 1px solid #e2e8f0;
      padding: 12px;
      border-radius: 4px;
      margin-bottom: 16px;
      font-size: 13px;
    }

    .error {
      color: #dc2626;
      font-size: 12px;
      margin-top: 4px;
      display: block;
    }

    .modal-footer {
      display: flex;
      gap: 12px;
      justify-content: flex-end;
      padding-top: 20px;
      border-top: 1px solid #e0e0e0;
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
      background: #e2e8f0;
      color: #334155;
    }

    .btn-submit {
      background: #2563eb;
      color: white;
    }

    .btn-submit:disabled {
      background: #94a3b8;
      cursor: not-allowed;
    }
  `]
})
export class FlashSaleFormComponent implements OnInit, OnDestroy, OnChanges {
  @Input() isOpen = false;
  @Input() isEditMode = false;
  @Input() flashSaleData: FlashSaleDto | null = null;
  @Output() save = new EventEmitter<FlashSaleDto>();
  @Output() close = new EventEmitter<void>();

  flashSaleForm!: FormGroup;
  loading = signal(false);
  private readonly destroy$ = new Subject<void>();

  private readonly fb = inject(FormBuilder);
  private readonly flashSaleService = inject(FlashSaleService);
  private readonly toast = inject(ToastService);

  ngOnInit() {
    this.initForm();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (!this.flashSaleForm) return;

    if (changes['isOpen']?.currentValue || changes['flashSaleData'] || changes['isEditMode']) {
      this.loading.set(false);
      this.applyFormState();
    }
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
    }, {
      validators: [this.dateRangeValidator(), this.startTimeValidator()]
    });

    this.applyFormState();
  }

  private applyFormState() {
    if (this.isEditMode && this.flashSaleData) {
      this.flashSaleForm.reset({
        title: this.flashSaleData.title ?? '',
        startTime: this.formatDateForInput(this.flashSaleData.startTime),
        endTime: this.formatDateForInput(this.flashSaleData.endTime),
        isActive: this.flashSaleData.isActive ?? true
      });
    } else {
      this.flashSaleForm.reset({
        title: '',
        startTime: '',
        endTime: '',
        isActive: true
      });
    }

    this.flashSaleForm.markAsPristine();
    this.flashSaleForm.markAsUntouched();
  }

  private dateRangeValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const startTime = control.get('startTime')?.value;
      const endTime = control.get('endTime')?.value;

      if (!startTime || !endTime) return null;

      const start = new Date(startTime);
      const end = new Date(endTime);

      if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime())) {
        return { invalidDate: true };
      }

      return end > start ? null : { invalidRange: true };
    };
  }

  private startTimeValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const startTime = control.get('startTime')?.value;
      if (!startTime) return null;

      const start = new Date(startTime);
      if (Number.isNaN(start.getTime())) {
        return { invalidDate: true };
      }

      if (!this.isEditMode && start.getTime() < Date.now() - 60_000) {
        return { startInPast: true };
      }

      return null;
    };
  }

  private formatDateForInput(date: string | Date): string {
    if (!date) return '';

    const parsed = new Date(date);
    if (Number.isNaN(parsed.getTime())) return '';

    const offset = parsed.getTimezoneOffset();
    const localDate = new Date(parsed.getTime() - offset * 60_000);
    return localDate.toISOString().slice(0, 16);
  }

  shouldShowFieldError(fieldName: string): boolean {
    const field = this.flashSaleForm.get(fieldName);
    return !!field && field.invalid && (field.dirty || field.touched);
  }

  getErrorMessage(fieldName: string): string {
    const field = this.flashSaleForm.get(fieldName);

    if (field?.hasError('required')) return 'Bat buoc';
    if (field?.hasError('minlength')) return `Toi thieu ${field.errors?.['minlength'].requiredLength} ky tu`;
    if (field?.hasError('maxlength')) return `Toi da ${field.errors?.['maxlength'].requiredLength} ky tu`;
    if (fieldName === 'startTime' && this.flashSaleForm.hasError('startInPast')) return 'Thoi gian bat dau khong duoc nam trong qua khu';
    if (fieldName === 'endTime' && this.flashSaleForm.hasError('invalidRange')) return 'Thoi gian ket thuc phai sau thoi gian bat dau';
    if (this.flashSaleForm.hasError('invalidDate')) return 'Ngay gio khong hop le';
    return 'Khong hop le';
  }

  onSubmit() {
    if (this.loading()) return;

    if (!this.flashSaleForm.valid) {
      this.flashSaleForm.markAllAsTouched();
      return;
    }

    this.loading.set(true);

    const formValue = this.flashSaleForm.getRawValue();
    const payload = {
      ...formValue,
      title: formValue.title.trim(),
      startTime: new Date(formValue.startTime).toISOString(),
      endTime: new Date(formValue.endTime).toISOString()
    };

    const request = this.isEditMode && this.flashSaleData
      ? this.flashSaleService.update(this.flashSaleData.flashSaleId, payload)
      : this.flashSaleService.create(payload);

    request.pipe(
      takeUntil(this.destroy$),
      finalize(() => {
        this.loading.set(false);
      })
    ).subscribe({
      next: (result) => {
        this.toast.success(this.isEditMode ? 'Cap nhat flash sale thanh cong' : 'Tao flash sale thanh cong');
        this.save.emit(result);
        this.onCancel();
      },
      error: (error) => {
        this.toast.error(error?.error?.message || 'Loi khi luu flash sale');
      }
    });
  }

  onCancel() {
    this.loading.set(false);
    this.close.emit();
  }
}
