import { Component, signal, computed, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../../core/services/product.service';
import { InventoryService } from '../../../core/services/inventory.service';
import { ToastService } from '../../../core/services/toast.service';
import { ProductDto } from '../../../core/models/product.model';

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './inventory.html',
})
export class Inventory implements OnInit {
  private productService = inject(ProductService);
  private inventoryService = inject(InventoryService);
  private toast = inject(ToastService);

  items = signal<ProductDto[]>([]);
  isLoading = signal(true);
  filterMode = signal<'all' | 'out' | 'low'>('all');

  filteredItems = computed(() => {
    const mode = this.filterMode();
    const list = this.items();
    if (mode === 'out') return list.filter((i) => i.stockQuantity === 0);
    if (mode === 'low') return list.filter((i) => i.stockQuantity > 0 && i.stockQuantity <= 5);
    return list;
  });

  outOfStock = computed(() => this.items().filter((i) => i.stockQuantity === 0).length);
  lowStock = computed(
    () => this.items().filter((i) => i.stockQuantity > 0 && i.stockQuantity <= 5).length,
  );
  totalValue = computed(() =>
    this.items().reduce((sum, i) => sum + i.stockQuantity * i.regularPrice, 0),
  );

  ngOnInit() {
    this.loadProducts();
  }

  loadProducts() {
    this.isLoading.set(true);
    this.productService.getAll({ page: 1, pageSize: 100 }).subscribe({
      next: (res) => {
        this.items.set(res.items.filter((p) => p.status !== 3));
        this.isLoading.set(false);
      },
      error: () => {
        this.toast.error('Không thể tải dữ liệu kho hàng');
        this.isLoading.set(false);
      },
    });
  }

  updateStock(product: ProductDto, delta: number) {
    const newQty = product.stockQuantity + delta;
    if (newQty < 0) {
      this.toast.error('Số lượng tồn kho không thể nhỏ hơn 0');
      return;
    }

    this.inventoryService.adjustStock({
      productId: product.productId,
      quantityChanged: delta,
      transactionType: 5, // 5: Điều chỉnh
      notes: 'Điều chỉnh nhanh từ quản lý kho'
    }).subscribe({
      next: () => {
        this.toast.success(`Cập nhật tồn kho sản phẩm "${product.name}" thành công`);
        this.items.update((list) =>
          list.map((p) =>
            p.productId === product.productId ? { ...p, stockQuantity: newQty } : p,
          ),
        );
      },
      error: () => this.toast.error('Có lỗi xảy ra khi cập nhật tồn kho'),
    });
  }
}
