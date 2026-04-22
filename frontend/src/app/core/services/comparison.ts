import { Injectable, signal, computed, inject } from '@angular/core';
import { ProductService } from './product.service';
import { ToastService } from './toast.service';

export interface CompareProduct {
  id: string;
  name: string;
  price: number;
  image: string;
  category: string;
  specs: Record<string, string>;
}

@Injectable({
  providedIn: 'root',
})
export class ComparisonService {
  private readonly productService = inject(ProductService);
  private readonly toastService = inject(ToastService);
  private readonly MAX_COMPARE = 4;
  private _selectedProducts = signal<CompareProduct[]>([]);

  readonly selectedProducts = this._selectedProducts.asReadonly();
  readonly selectedCount = computed(() => this._selectedProducts().length);
  readonly canCompare = computed(() => this._selectedProducts().length >= 2);
  readonly isFull = computed(() => this._selectedProducts().length >= this.MAX_COMPARE);

  isSelected(productId: string): boolean {
    return this._selectedProducts().some((p) => p.id === productId);
  }

  toggleProduct(product: CompareProduct): void {
    if (this.isSelected(product.id)) {
      this._selectedProducts.update((products) => products.filter((p) => p.id !== product.id));
    } else {
      if (this.isFull()) {
        this._selectedProducts.update((products) => [...products.slice(1), product]);
      } else {
        this._selectedProducts.update((products) => [...products, product]);
      }
    }
  }

  /**
   * Toggles a product and automatically fetches full details (specs) if adding
   */
  toggleProductWithFetch(id: string, basicInfo: Partial<CompareProduct>): void {
    if (this.isSelected(id)) {
      this.removeProduct(id);
      return;
    }

    if (this.isFull()) {
      this.toastService.warning('Chỉ có thể so sánh tối đa 4 sản phẩm');
      return;
    }

    // 1. Optimistic Add: Thêm dữ liệu cơ bản ngay lập tức để icon tích hiện lên luôn
    const placeholder: CompareProduct = {
      id: id,
      name: basicInfo.name || 'Sản phẩm',
      price: basicInfo.price || 0,
      image: basicInfo.image || '',
      category: basicInfo.category || '',
      specs: {},
    };
    this._selectedProducts.update((list) => [...list, placeholder]);

    // 2. Fetch đầy đủ specs và cập nhật lại bản ghi đó
    this.productService.getFullById(id).subscribe({
      next: (full) => {
        let specsObj: Record<string, string> = {};
        if (full.product.specifications) {
          try {
            specsObj = JSON.parse(full.product.specifications);
          } catch {
            specsObj = {};
          }
        }
        full.specs.forEach((s) => (specsObj[s.specKey] = s.specValue));

        this._selectedProducts.update((list) =>
          list.map((p) =>
            p.id === id
              ? {
                  ...p,
                  name: full.product.name,
                  price: full.product.salePrice || full.product.regularPrice,
                  image:
                    full.images.find((i) => i.isPrimary)?.imageUrl ||
                    full.images[0]?.imageUrl ||
                    p.image,
                  category: full.product.categoryName,
                  specs: specsObj,
                }
              : p,
          ),
        );
      },
      error: () => {
        // Nếu lỗi fetch chi tiết, có thể xóa hoặc giữ lại basic info tùy logic
        // Ở đây ta giữ lại để không làm gián đoạn trải nghiệm
      },
    });
  }

  removeProduct(productId: string): void {
    this._selectedProducts.update((products) => products.filter((p) => p.id !== productId));
  }

  clearAll(): void {
    this._selectedProducts.set([]);
  }
}
