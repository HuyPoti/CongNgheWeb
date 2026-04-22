import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../core/pipes/translate.pipe';
import { ComparisonService } from '../../core/services/comparison';
import { ProductService } from '../../core/services/product.service';
import { ProductListItemDto } from '../../core/models/product.model';
import { debounceTime, distinctUntilChanged, Subject, switchMap, map } from 'rxjs';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-comparison',
  imports: [RouterLink, CommonModule, TranslatePipe, FormsModule],
  templateUrl: './comparison.html',
  styles: `
    .sticky-header {
      position: sticky;
      top: 64px;
      z-index: 20;
      background: white;
      border-bottom: 1px solid #e5e7eb;
    }
    :host-context(.dark) .sticky-header {
      background: #121827;
      border-bottom: 1px solid rgba(255, 255, 255, 0.1);
    }
  `,
})
export class Comparison {
  comparisonService = inject(ComparisonService);
  productService = inject(ProductService);

  searchQuery = '';
  searchResults: ProductListItemDto[] = [];
  isSearching = false;
  showDifferencesOnly = false;
  private searchSubject = new Subject<string>();

  constructor() {
    this.searchSubject
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((query) => {
          if (!query.trim()) {
            this.searchResults = [];
            return [];
          }
          this.isSearching = true;
          return this.productService
            .fetchClientProducts({ keyword: query, pageSize: 5 })
            .pipe(map((res) => res.items));
        }),
      )
      .subscribe((items) => {
        this.searchResults = items.filter((p) => !this.comparisonService.isSelected(p.id));
        this.isSearching = false;
      });
  }

  onSearch(query: string) {
    this.searchSubject.next(query);
  }

  addToCompare(product: ProductListItemDto) {
    this.comparisonService.toggleProductWithFetch(product.id, {
      name: product.name,
      price: product.price,
      image: product.thumbnailUrl || '/assets/placeholder.png',
      category: product.categoryName,
    });
    this.searchQuery = '';
    this.searchResults = [];
  }

  getSpecValue(productId: string, specKey: string): string {
    const product = this.comparisonService.selectedProducts().find((p) => p.id === productId);
    if (!product) return '—';
    return product.specs[specKey] || '—';
  }

  getSpecKeys(): string[] {
    const products = this.comparisonService.selectedProducts();
    const allKeys = new Set<string>();
    products.forEach((p) => {
      Object.keys(p.specs).forEach((k) => allKeys.add(k));
    });

    let keys = Array.from(allKeys).sort();

    if (this.showDifferencesOnly && products.length > 1) {
      keys = keys.filter((key) => this.isDifferent(key));
    }

    return keys;
  }

  isDifferent(key: string): boolean {
    const products = this.comparisonService.selectedProducts();
    if (products.length < 2) return false;

    const firstValue = (products[0].specs[key] || '').trim().toLowerCase();
    return products.some((p) => (p.specs[key] || '').trim().toLowerCase() !== firstValue);
  }
}
