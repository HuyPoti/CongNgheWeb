import { Component, EventEmitter, Input, OnDestroy, OnInit, Output, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { of, Subject } from 'rxjs';
import { catchError, debounceTime, distinctUntilChanged, finalize, switchMap, takeUntil, tap } from 'rxjs/operators';
import { ProductDto } from '../../../core/models/product.model';
import { ProductService } from '../../../core/services/product.service';
import { FlashSaleService } from '../../../core/services/flash-sale.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-flash-sale-items',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="modal-overlay" *ngIf="isOpen" (click)="onCancel()">
      <div class="modal-content" (click)="$event.stopPropagation()">
        <div class="modal-header">
          <h2>Quan Ly San Pham Flash Sale: {{ flashSaleData?.title }}</h2>
          <button class="close-btn" type="button" (click)="onCancel()">x</button>
        </div>

        <div class="modal-body">
          <div class="add-item-section">
            <form [formGroup]="addItemForm" (ngSubmit)="onAddItem()">
              <div class="search-block">
                <label for="flash-sale-product-search">Tim san pham</label>
                <input
                  id="flash-sale-product-search"
                  type="text"
                  formControlName="productSearch"
                  autocomplete="off"
                  placeholder="Nhap ten hoac SKU san pham" />

                <div class="search-state" *ngIf="searchingProducts()">Dang tim san pham...</div>

                <div class="search-results" *ngIf="showSearchResults()">
                  <button
                    *ngFor="let product of searchResults()"
                    type="button"
                    class="search-item"
                    (click)="selectProduct(product)">
                    <div class="search-item-main">
                      <strong>{{ product.name }}</strong>
                      <span *ngIf="product.sku">SKU: {{ product.sku }}</span>
                    </div>
                    <div class="search-item-meta">
                      <span>Sale: {{ product.salePrice ?? 0 | number:'1.0-0' }} VND</span>
                      <span>Ton: {{ product.stockQuantity }}</span>
                    </div>
                  </button>
                </div>

                <div class="search-state warning" *ngIf="showEmptySearchState()">
                  Khong tim thay san pham phu hop. Chi hien san pham co sale price va chua nam trong flash sale nay.
                </div>

                <span class="error" *ngIf="shouldShowProductError()">
                  Vui long chon mot san pham tu danh sach
                </span>
              </div>

              <div class="selected-product" *ngIf="selectedProduct() as product">
                <div class="selected-product-main">
                  <strong>{{ product.name }}</strong>
                  <span *ngIf="product.sku">SKU: {{ product.sku }}</span>
                </div>
                <div class="selected-product-meta">
                  <span>Gia niem yet: {{ product.regularPrice | number:'1.0-0' }} VND</span>
                  <span>Gia sale hien tai: {{ product.salePrice ?? 0 | number:'1.0-0' }} VND</span>
                  <span>Ton kho: {{ product.stockQuantity }}</span>
                </div>
                <button type="button" class="btn-clear" (click)="clearSelectedProduct()">Bo chon</button>
              </div>

              <div class="hint">
                Flash price phai nho hon gia sale hien tai cua san pham.
              </div>

              <div class="form-row">
                <div class="form-group">
                  <label for="flash-sale-price">Gia Flash Sale</label>
                  <input id="flash-sale-price" type="number" formControlName="flashPrice" placeholder="Gia sau giam" />
                  <span class="error" *ngIf="addItemForm.get('flashPrice')?.errors && addItemForm.get('flashPrice')?.touched">
                    {{ addItemForm.get('flashPrice')?.hasError('required') ? 'Bat buoc' : 'Phai > 0' }}
                  </span>
                </div>

                <div class="form-group">
                  <label for="flash-sale-stock-limit">Luot Hang</label>
                  <input id="flash-sale-stock-limit" type="number" formControlName="stockLimit" placeholder="So luong toi da" />
                  <span class="error" *ngIf="addItemForm.get('stockLimit')?.errors && addItemForm.get('stockLimit')?.touched">
                    {{ addItemForm.get('stockLimit')?.hasError('required') ? 'Bat buoc' : 'Phai > 0' }}
                  </span>
                </div>

                <button
                  type="submit"
                  class="btn-add"
                  [disabled]="addLoading || addItemForm.invalid || !selectedProduct() || !addItemForm.get('productId')?.value">
                  {{ addLoading ? 'Dang them...' : 'Them vao Flash Sale' }}
                </button>
              </div>
            </form>
          </div>

          <div class="items-list">
            <h3>Danh Sach San Pham</h3>
            <div *ngIf="flashSaleData?.items?.length; else noItems">
              <div *ngFor="let item of flashSaleData.items" class="item-card">
                <div class="item-header">
                  <div class="product-info">
                    <strong>Ten San Pham: {{ item.productName }}</strong>
                    <p>Gia: {{ item.flashPrice | number:'1.0-0' }} VND</p>
                  </div>
                  <button class="btn-remove" type="button" (click)="onRemoveItem(item.productId)" [disabled]="removeLoading">
                    {{ removeLoading ? '...' : 'x' }}
                  </button>
                </div>

                <div class="progress-section">
                  <div class="progress-label">
                    <span>Da Ban: {{ item.soldCount || 0 }} / {{ item.stockLimit }}</span>
                    <span [class.sold-out]="item.isSoldOut">{{ item.isSoldOut ? 'Het Hang' : '' }}</span>
                  </div>
                  <div class="progress-bar">
                    <div class="progress-fill" [style.width.%]="getProgressPercent(item)"></div>
                  </div>
                </div>
              </div>
            </div>
            <ng-template #noItems>
              <p class="empty-message">Chua co san pham nao</p>
            </ng-template>
          </div>
        </div>

        <div class="modal-footer">
          <button type="button" class="btn-cancel" (click)="onCancel()">Dong</button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .modal-overlay {
      position: fixed;
      inset: 0;
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
      max-width: 760px;
      max-height: 80vh;
      overflow-x: hidden;
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
      gap: 14px;
    }

    .search-block {
      position: relative;
      display: flex;
      flex-direction: column;
      gap: 6px;
    }

    .search-results {
      max-height: 240px;
      overflow-y: auto;
      border: 1px solid #d0d7de;
      border-radius: 8px;
      background: #fff;
      box-shadow: 0 10px 24px rgba(15, 23, 42, 0.08);
    }

    .search-item {
      width: 100%;
      border: none;
      border-bottom: 1px solid #eef2f7;
      background: transparent;
      padding: 12px;
      text-align: left;
      cursor: pointer;
    }

    .search-item:last-child {
      border-bottom: none;
    }

    .search-item:hover {
      background: #f8fafc;
    }

    .search-item-main,
    .search-item-meta,
    .selected-product-main,
    .selected-product-meta {
      display: flex;
      gap: 8px;
      flex-wrap: wrap;
    }

    .search-item-main strong,
    .selected-product-main strong {
      color: #0f172a;
    }

    .search-item-main span,
    .search-item-meta span,
    .selected-product-main span,
    .selected-product-meta span,
    .search-state,
    .hint {
      font-size: 12px;
      color: #64748b;
    }

    .search-state {
      padding: 10px 12px;
      border-radius: 6px;
      background: #fff;
      border: 1px solid #e2e8f0;
    }

    .search-state.warning {
      color: #92400e;
      background: #fff7ed;
      border-color: #fed7aa;
    }

    .selected-product {
      display: flex;
      flex-direction: column;
      gap: 8px;
      padding: 12px;
      border-radius: 8px;
      background: #ffffff;
      border: 1px solid #dbeafe;
    }

    .btn-clear {
      align-self: flex-start;
      padding: 6px 10px;
      border: 1px solid #cbd5e1;
      border-radius: 999px;
      background: #fff;
      color: #334155;
      cursor: pointer;
      font-size: 12px;
      font-weight: 600;
    }

    .form-row {
      display: grid;
      grid-template-columns: repeat(2, minmax(0, 1fr));
      gap: 12px;
      align-items: end;
    }

    .form-group {
      display: flex;
      flex-direction: column;
      min-width: 0;
    }

    label {
      font-weight: 600;
      margin-bottom: 6px;
      color: #333;
      font-size: 13px;
    }

    input {
      width: 100%;
      min-width: 0;
      box-sizing: border-box;
      padding: 10px 12px;
      border: 1px solid #d0d0d0;
      border-radius: 8px;
      font-size: 13px;
      font-family: inherit;
      background: #fff;
    }

    input:focus {
      outline: none;
      border-color: #007bff;
      box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.1);
    }

    .btn-add {
      grid-column: 1 / -1;
      width: 100%;
      min-height: 42px;
      padding: 8px 16px;
      background: #28a745;
      color: white;
      border: none;
      border-radius: 8px;
      cursor: pointer;
      font-weight: 700;
      font-size: 13px;
    }

    .btn-add:hover:not(:disabled) {
      background: #218838;
    }

    .btn-add:disabled {
      background: #ccc;
      cursor: not-allowed;
    }

    .error {
      color: #dc3545;
      font-size: 12px;
      margin-top: 4px;
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
      overflow: hidden;
    }

    .item-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 12px;
      margin-bottom: 12px;
    }

    .product-info {
      flex: 1;
      min-width: 0;
    }

    .product-info strong {
      display: block;
      font-size: 14px;
      color: #333;
      margin-bottom: 4px;
      word-break: break-word;
    }

    .product-info p {
      margin: 0;
      font-size: 13px;
      color: #666;
      word-break: break-word;
    }

    .btn-remove {
      flex: 0 0 auto;
      min-width: 36px;
      min-height: 36px;
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
      gap: 8px;
      flex-wrap: wrap;
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

    @media (min-width: 960px) {
      .form-row {
        grid-template-columns: repeat(2, minmax(0, 1fr)) auto;
      }

      .btn-add {
        grid-column: auto;
        width: auto;
        min-width: 170px;
      }
    }

    @media (max-width: 639px) {
      .modal-content {
        width: calc(100% - 24px);
        max-height: 88vh;
      }

      .modal-header,
      .modal-body,
      .modal-footer {
        padding-left: 14px;
        padding-right: 14px;
      }

      .item-header {
        align-items: flex-start;
      }
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
  removeLoading = false;
  searchResults = signal<ProductDto[]>([]);
  selectedProduct = signal<ProductDto | null>(null);
  searchingProducts = signal(false);
  private readonly destroy$ = new Subject<void>();

  private readonly fb = inject(FormBuilder);
  private readonly flashSaleService = inject(FlashSaleService);
  private readonly productService = inject(ProductService);
  private readonly toast = inject(ToastService);

  ngOnInit() {
    this.initForm();
    this.bindProductSearch();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initForm() {
    this.addItemForm = this.fb.group({
      productSearch: [''],
      productId: ['', Validators.required],
      flashPrice: ['', [Validators.required, Validators.min(0.01)]],
      stockLimit: ['', [Validators.required, Validators.min(1)]]
    });
  }

  private bindProductSearch() {
    this.addItemForm.get('productSearch')?.valueChanges.pipe(
      tap((rawValue) => {
        const keyword = String(rawValue ?? '').trim();
        const selected = this.selectedProduct();

        if (!selected) {
          return;
        }

        if (keyword !== selected.name) {
          this.selectedProduct.set(null);
          this.addItemForm.patchValue({ productId: '' }, { emitEvent: false });
        }
      }),
      debounceTime(250),
      distinctUntilChanged(),
      switchMap((rawValue) => {
        const keyword = String(rawValue ?? '').trim();

        if (!keyword) {
          this.searchResults.set([]);
          this.searchingProducts.set(false);
          if (!this.selectedProduct()) {
            this.addItemForm.patchValue({ productId: '' }, { emitEvent: false });
          }
          return of(null);
        }

        if (this.selectedProduct()?.name === keyword) {
          return of(null);
        }

        this.searchingProducts.set(true);

        return this.productService.getAll({ keyword, page: 1, pageSize: 8 }).pipe(
          catchError(() => {
            this.toast.error('Khong the tim san pham');
            return of({ items: [], totalCount: 0, page: 1, pageSize: 8 });
          }),
          finalize(() => {
            this.searchingProducts.set(false);
          })
        );
      }),
      takeUntil(this.destroy$)
    ).subscribe((response) => {
      if (!response) return;

      const items = response.items.filter((product) => this.isSelectableProduct(product));
      this.searchResults.set(items);
    });
  }

  private isSelectableProduct(product: ProductDto): boolean {
    const alreadyExists = !!this.flashSaleData?.items?.some((item: any) => item.productId === product.productId);
    return !alreadyExists && !!product.salePrice && product.salePrice > 0;
  }

  selectProduct(product: ProductDto) {
    this.selectedProduct.set(product);
    this.searchResults.set([]);
    this.addItemForm.patchValue({
      productSearch: product.name,
      productId: product.productId
    }, { emitEvent: false });
    this.addItemForm.get('productId')?.markAsTouched();
  }

  clearSelectedProduct() {
    this.selectedProduct.set(null);
    this.searchResults.set([]);
    this.addItemForm.patchValue({
      productSearch: '',
      productId: ''
    });
  }

  shouldShowProductError(): boolean {
    const productIdControl = this.addItemForm.get('productId');
    return !!productIdControl && productIdControl.invalid && (productIdControl.touched || productIdControl.dirty);
  }

  showSearchResults(): boolean {
    return this.searchResults().length > 0;
  }

  showEmptySearchState(): boolean {
    const keyword = String(this.addItemForm.get('productSearch')?.value ?? '').trim();
    return !!keyword && !this.searchingProducts() && this.searchResults().length === 0 && !this.selectedProduct();
  }

  onAddItem() {
    if (this.addLoading) {
      return;
    }

    if (!this.selectedProduct()) {
      this.addItemForm.get('productId')?.markAsTouched();
    }

    const productId = String(this.addItemForm.get('productId')?.value ?? '').trim();

    if (this.addItemForm.invalid || !this.selectedProduct() || !productId || !this.flashSaleData) {
      return;
    }

    this.addLoading = true;

    const { flashPrice, stockLimit } = this.addItemForm.getRawValue();

    this.flashSaleService.addItem(this.flashSaleData.flashSaleId.toString(), {
      productId,
      flashPrice,
      stockLimit
    }).pipe(
      takeUntil(this.destroy$),
      finalize(() => {
        this.addLoading = false;
      })
    ).subscribe({
      next: () => {
        const product = this.selectedProduct();
        this.addItemForm.reset({
          productSearch: '',
          productId: '',
          flashPrice: '',
          stockLimit: ''
        });
        this.searchResults.set([]);
        this.selectedProduct.set(null);
        this.flashSaleData.items = [
          ...(this.flashSaleData.items || []),
          {
            productId,
            productName: product?.name ?? 'San pham',
            flashPrice,
            stockLimit,
            soldCount: 0,
            isSoldOut: false
          }
        ];
        this.toast.success('Da them san pham vao flash sale');
        this.itemsUpdated.emit();
      },
      error: (error) => {
        this.toast.error(error?.error?.message || 'Loi khi them san pham');
      }
    });
  }

  onRemoveItem(productId: string) {
    if (!confirm('Ban chac chan muon xoa san pham nay?')) return;

    this.removeLoading = true;

    this.flashSaleService.removeItem(this.flashSaleData.flashSaleId.toString(), productId.toString())
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.removeLoading = false;
        })
      ).subscribe({
        next: () => {
          this.flashSaleData.items = this.flashSaleData.items.filter((i: any) => i.productId !== productId);
          this.toast.success('Da xoa san pham khoi flash sale');
          this.itemsUpdated.emit();
        },
        error: (error) => {
          this.toast.error(error?.error?.message || 'Loi khi xoa san pham');
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
