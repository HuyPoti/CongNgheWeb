import { Injectable, signal, computed } from '@angular/core';

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
  private readonly MAX_COMPARE = 2;
  private _selectedProducts = signal<CompareProduct[]>([]);

  readonly selectedProducts = this._selectedProducts.asReadonly();
  readonly selectedCount = computed(() => this._selectedProducts().length);
  readonly canCompare = computed(() => this._selectedProducts().length === this.MAX_COMPARE);
  readonly isFull = computed(() => this._selectedProducts().length >= this.MAX_COMPARE);

  isSelected(productId: string): boolean {
    return this._selectedProducts().some(p => p.id === productId);
  }

  toggleProduct(product: CompareProduct): void {
    if (this.isSelected(product.id)) {
      this._selectedProducts.update(products => products.filter(p => p.id !== product.id));
    } else {
      if (this.isFull()) {
        // Replace the first selected product
        this._selectedProducts.update(products => [products[1], product]);
      } else {
        this._selectedProducts.update(products => [...products, product]);
      }
    }
  }

  removeProduct(productId: string): void {
    this._selectedProducts.update(products => products.filter(p => p.id !== productId));
  }

  clearAll(): void {
    this._selectedProducts.set([]);
  }
}
