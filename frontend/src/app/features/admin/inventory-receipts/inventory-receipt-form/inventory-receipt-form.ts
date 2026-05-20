import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { InventoryService } from '../../../../core/services/inventory.service';
import { SupplierService } from '../../../../core/services/supplier.service';
import { ProductService } from '../../../../core/services/product.service';
import { ToastService } from '../../../../core/services/toast.service';
import { Supplier } from '../../../../core/models/supplier.model';
import { ProductDto, PagedProductResponse } from '../../../../core/models/product.model';

@Component({
  selector: 'app-inventory-receipt-form',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, ReactiveFormsModule],
  templateUrl: './inventory-receipt-form.html'
})
export class InventoryReceiptForm implements OnInit {
  private fb = inject(FormBuilder);
  private inventoryService = inject(InventoryService);
  private supplierService = inject(SupplierService);
  private productService = inject(ProductService);
  private router = inject(Router);
  private toast = inject(ToastService);

  receiptForm: FormGroup;
  suppliers = signal<Supplier[]>([]);
  products = signal<ProductDto[]>([]);
  isSaving = signal<boolean>(false);

  constructor() {
    this.receiptForm = this.fb.group({
      supplierId: ['', Validators.required],
      notes: [''],
      items: this.fb.array([], Validators.required)
    });
  }

  ngOnInit() {
    this.loadSuppliers();
    this.loadProducts();
  }

  get items() {
    return this.receiptForm.get('items') as FormArray;
  }

  loadSuppliers() {
    this.supplierService.getSuppliers().subscribe({
      next: (data: Supplier[]) => this.suppliers.set(data.filter(s => s.isActive)),
      error: (err: unknown) => console.error('Lỗi tải NCC', err)
    });
  }

  loadProducts() {
    this.productService.getAll({ pageSize: 100 }).subscribe({
      next: (res: PagedProductResponse) => this.products.set(res.items),
      error: (err: unknown) => console.error('Lỗi tải sản phẩm', err)
    });
  }

  addItem() {
    const itemGroup = this.fb.group({
      productId: ['', Validators.required],
      quantity: [1, [Validators.required, Validators.min(1)]],
      unitPrice: [0, [Validators.required, Validators.min(0)]]
    });
    this.items.push(itemGroup);
  }

  removeItem(index: number) {
    this.items.removeAt(index);
  }

  getTotalAmount(): number {
    return this.items.controls.reduce((total, control) => {
      const q = control.get('quantity')?.value || 0;
      const p = control.get('unitPrice')?.value || 0;
      return total + (q * p);
    }, 0);
  }

  saveReceipt() {
    if (this.receiptForm.invalid || this.items.length === 0) {
      this.toast.warning('Vui lòng kiểm tra lại thông tin. Đảm bảo chọn ít nhất 1 sản phẩm.');
      return;
    }

    // Check for duplicate products
    const productIds = this.items.value.map((i: { productId: string }) => i.productId);
    const hasDuplicates = new Set(productIds).size !== productIds.length;
    if (hasDuplicates) {
      this.toast.warning('Có sản phẩm trùng lặp trong phiếu nhập. Vui lòng gộp lại.');
      return;
    }

    this.isSaving.set(true);
    this.inventoryService.createReceipt(this.receiptForm.value).subscribe({
      next: (res: { receiptId: string }) => {
        this.toast.success('Tạo phiếu nháp thành công!');
        this.router.navigate(['/admin/inventory-receipts', res.receiptId]);
      },
      error: (err: { error?: { message?: string }; message: string }) => {
        console.error('Lỗi tạo phiếu', err);
        this.toast.error('Lỗi: ' + (err.error?.message || err.message));
        this.isSaving.set(false);
      }
    });
  }
}
