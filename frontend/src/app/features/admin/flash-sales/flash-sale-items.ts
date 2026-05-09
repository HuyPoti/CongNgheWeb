import { Component, OnInit, OnDestroy, Input, Output, EventEmitter, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { FlashSaleService } from '../../../core/services/flash-sale.service';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-flash-sale-items',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="modal-overlay" *ngIf="isOpen" (click)="onCancel()">
      <div class="modal-content" (click)="$event.stopPropagation()">
        <div class="modal-header">
          <h2>Quản Lý Sản Phẩm Flash Sale: {{ flashSaleData?.title }}</h2>
          <button class="close-btn" (click)="onCancel()">✕</button>
        </div>

        <div class="modal-body">
          <div class="add-item-section">
            <form [formGroup]="addItemForm" (ngSubmit)="onAddItem()">
              <div class="form-row">
                <div class="form-group">
                  <label>ID Sản Phẩm</label>
                  <input type="number" formControlName="productId" placeholder="Nhập ID sản phẩm" />
                  <span class="error" *ngIf="addItemForm.get('productId')?.errors && addItemForm.get('productId')?.touched">
                    Bắt buộc
                  </span>
                </div>

                <div class="form-group">
                  <label>Giá Flash Sale</label>
                  <input type="number" formControlName="flashPrice" placeholder="Giá sau giảm" />
                  <span class="error" *ngIf="addItemForm.get('flashPrice')?.errors && addItemForm.get('flashPrice')?.touched">
                    {{ addItemForm.get('flashPrice')?.hasError('required') ? 'Bắt buộc' : 'Phải > 0' }}
                  </span>
                </div>

                <div class="form-group">
                  <label>Lượt Hàng</label>
                  <input type="number" formControlName="stockLimit" placeholder="Số lượng tối đa" />
                  <span class="error" *ngIf="addItemForm.get('stockLimit')?.errors && addItemForm.get('stockLimit')?.touched">
                    {{ addItemForm.get('stockLimit')?.hasError('required') ? 'Bắt buộc' : 'Phải > 0' }}
                  </span>
                </div>

                <button type="submit" class="btn-add" [disabled]="addLoading || !addItemForm.valid">
                  {{ addLoading ? 'Đang thêm...' : 'Thêm' }}
                </button>
              </div>
            </form>
            <span class="error-message" *ngIf="addError">{{ addError }}</span>
          </div>

          <div class="items-list">
            <h3>Danh Sách Sản Phẩm</h3>
            <div *ngIf="flashSaleData?.items?.length; else noItems">
              <div *ngFor="let item of flashSaleData.items" class="item-card">
                <div class="item-header">
                  <div class="product-info">
                    <strong>Sản Phẩm ID: {{ item.productId }}</strong>
                    <p>Giá: {{ item.flashPrice | number:'1.0-0' }} VND</p>
                  </div>
                  <button class="btn-remove" (click)="onRemoveItem(item.productId)" 
                    [disabled]="removeLoading">
                    {{ removeLoading ? '⏳' : '✕' }}
                  </button>
                </div>
                
                <div class="progress-section">
                  <div class="progress-label">
                    <span>Đã Bán: {{ item.soldCount || 0 }} / {{ item.stockLimit }}</span>
                    <span [class.sold-out]="item.isSoldOut">{{ item.isSoldOut ? '✓ Hết Hàng' : '' }}</span>
                  </div>
                  <div class="progress-bar">
                    <div class="progress-fill" [style.width.%]="getProgressPercent(item)"></div>
                  </div>
                </div>
              </div>
            </div>
            <ng-template #noItems>
              <p class="empty-message">Chưa có sản phẩm nào</p>
            </ng-template>
          </div>
        </div>

        <div class="modal-footer">
          <button type="button" class="btn-cancel" (click)="onCancel()">Đóng</button>
        </div>
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
      max-width: 700px;
      max-height: 80vh;
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
      font-size: 16px;
      color: #333;
    }

    .close-btn {
      background: none;
      border: none;
      font-size: 24px;
      cursor: pointer;
      color: #666;
    }

    .modal-body {
      padding: 20px;
    }

    .add-item-section {
      background: #f8f9fa;
      padding: 16px;
      border-radius: 6px;
      margin-bottom: 24px;
    }

    form {
      display: flex;
      flex-direction: column;
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr 1fr auto;
      gap: 12px;
      align-items: flex-end;
    }

    .form-group {
      display: flex;
      flex-direction: column;
    }

    label {
      font-weight: 600;
      margin-bottom: 6px;
      color: #333;
      font-size: 13px;
    }

    input {
      padding: 8px;
      border: 1px solid #d0d0d0;
      border-radius: 4px;
      font-size: 13px;
      font-family: inherit;
    }

    input:focus {
      outline: none;
      border-color: #007bff;
      box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.1);
    }

    .btn-add {
      padding: 8px 16px;
      background: #28a745;
      color: white;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-weight: 600;
      font-size: 13px;
    }

    .btn-add:hover:not(:disabled) {
      background: #218838;
    }

    .btn-add:disabled {
      background: #ccc;
      cursor: not-allowed;
    }

    .error, .error-message {
      color: #dc3545;
      font-size: 12px;
      margin-top: 4px;
    }

    .error-message {
      background: #f8d7da;
      border: 1px solid #f5c6cb;
      padding: 8px;
      border-radius: 4px;
      margin-bottom: 12px;
    }

    .items-list h3 {
      margin: 0 0 16px 0;
      font-size: 15px;
      color: #333;
    }

    .item-card {
      background: #f8f9fa;
      border: 1px solid #e0e0e0;
      border-radius: 6px;
      padding: 12px;
      margin-bottom: 12px;
    }

    .item-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 12px;
    }

    .product-info {
      flex: 1;
    }

    .product-info strong {
      display: block;
      font-size: 14px;
      color: #333;
      margin-bottom: 4px;
    }

    .product-info p {
      margin: 0;
      font-size: 13px;
      color: #666;
    }

    .btn-remove {
      padding: 6px 10px;
      background: #dc3545;
      color: white;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-weight: 600;
    }

    .btn-remove:hover:not(:disabled) {
      background: #c82333;
    }

    .btn-remove:disabled {
      background: #ccc;
      cursor: not-allowed;
    }

    .progress-section {
      margin-top: 12px;
    }

    .progress-label {
      display: flex;
      justify-content: space-between;
      font-size: 12px;
      color: #666;
      margin-bottom: 6px;
    }

    .sold-out {
      color: #dc3545;
      font-weight: 600;
    }

    .progress-bar {
      width: 100%;
      height: 20px;
      background: #e0e0e0;
      border-radius: 10px;
      overflow: hidden;
    }

    .progress-fill {
      height: 100%;
      background: linear-gradient(90deg, #28a745, #20c997);
      transition: width 0.3s ease;
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-size: 11px;
      font-weight: 600;
    }

    .empty-message {
      text-align: center;
      color: #999;
      padding: 20px;
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

    .btn-cancel {
      padding: 10px 20px;
      background: #e0e0e0;
      color: #333;
      border: none;
      border-radius: 4px;
      cursor: pointer;
      font-weight: 600;
      font-size: 14px;
    }

    .btn-cancel:hover {
      background: #d0d0d0;
    }
  `]
})
export class FlashSaleItemsComponent implements OnInit, OnDestroy {
  @Input() isOpen = false;
  @Input() flashSaleData: any;
  @Output() close = new EventEmitter<void>();
  @Output() itemsUpdated = new EventEmitter<void>();

  addItemForm!: FormGroup;
  addLoading = false;
  addError = '';
  removeLoading = false;
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
    this.addItemForm = this.fb.group({
      productId: ['', Validators.required],
      flashPrice: ['', [Validators.required, Validators.min(0.01)]],
      stockLimit: ['', [Validators.required, Validators.min(1)]]
    });
  }

  onAddItem() {
    if (!this.addItemForm.valid || !this.flashSaleData) return;

    this.addLoading = true;
    this.addError = '';

    const { productId, flashPrice, stockLimit } = this.addItemForm.value;

    this.flashSaleService.addItem(this.flashSaleData.flashSaleId.toString(), {
      productId: productId.toString(),
      flashPrice,
      stockLimit
    }).pipe(takeUntil(this.destroy$)).subscribe({
      next: () => {
        this.addLoading = false;
        this.addItemForm.reset();
        this.flashSaleData.items = [...(this.flashSaleData.items || []), { productId, flashPrice, stockLimit, soldCount: 0, isSoldOut: false }];
        this.itemsUpdated.emit();
      },
      error: (error) => {
        this.addLoading = false;
        this.addError = error?.error?.message || 'Lỗi khi thêm sản phẩm';
      }
    });
  }

  onRemoveItem(productId: number) {
    if (!confirm('Bạn chắc chắn muốn xóa sản phẩm này?')) return;

    this.removeLoading = true;

    this.flashSaleService.removeItem(this.flashSaleData.flashSaleId.toString(), productId.toString())
      .pipe(takeUntil(this.destroy$)).subscribe({
        next: () => {
          this.removeLoading = false;
          this.flashSaleData.items = this.flashSaleData.items.filter((i: any) => i.productId !== productId);
          this.itemsUpdated.emit();
        },
        error: (error) => {
          this.removeLoading = false;
          alert(error?.error?.message || 'Lỗi khi xóa sản phẩm');
        }
      });
  }

  getProgressPercent(item: any): number {
    if (!item.stockLimit) return 0;
    return Math.min((item.soldCount || 0) / item.stockLimit * 100, 100);
  }

  onCancel() {
    this.close.emit();
  }
}
