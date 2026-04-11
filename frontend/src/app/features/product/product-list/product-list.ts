import { Component, inject, OnInit } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ComparisonService, CompareProduct } from '../../../core/services/comparison';
import { ProductCard, ProductListItemDto } from '../../../core/models/product.model';
import { ProductService } from '../../../core/services/product.service';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [RouterLink, CommonModule, TranslatePipe],
  templateUrl: './product-list.html',
  styles: ``,
})
export class ProductList implements OnInit {
  // ── DI ──────────────────────────────────────────────────────────
  comparisonService = inject(ComparisonService);
  private router = inject(Router);
  private productService = inject(ProductService);

  // ── State ────────────────────────────────────────────────────────
  featuredProducts: ProductCard[] = [];
  categories: string[] = ['settings.all'];
  selectedCategory = 'settings.all';
  totalCount = 0;
  isLoading = true;

  priceRanges = [
    { key: 'product.filter_price_under_10', label: 'Dưới 10 triệu' },
    { key: 'product.filter_price_10_20', label: '10 - 20 triệu' },
    { key: 'product.filter_price_20_30', label: '20 - 30 triệu' },
    { key: 'product.filter_price_30_50', label: '30 - 50 triệu' },
    { key: 'product.filter_price_over_50', label: 'Trên 50 triệu' }
  ];

  sortOptions = [
    { key: 'product.sort_newest', label: 'Mới nhất' },
    { key: 'product.sort_price_asc', label: 'Giá thấp - cao' },
    { key: 'product.sort_price_desc', label: 'Giá cao - thấp' },
    { key: 'product.sort_best_selling', label: 'Bán chạy' }
  ];

  // ── Lifecycle ────────────────────────────────────────────────────
  ngOnInit(): void {
    this.loadProducts();
  }

  // ── Data loaders ─────────────────────────────────────────────────
  private loadProducts(): void {
    this.productService.fetchClientProducts(1, 100).subscribe({
      next: (res) => {
        this.totalCount = res.totalCount;
        this.featuredProducts = res.items.map((p: ProductListItemDto) => ({
          id: p.id,
          name: p.name,
          price: p.price,
          image: p.thumbnailUrl || 'https://ttgshop.vn/media/product/250_1072100124_dsc09857_copy.jpg',
          category: p.categoryName,
          specs: {},
        }));
        const unique = Array.from(
          new Set(res.items.map((p: ProductListItemDto) => p.categoryName)),
        );
        this.categories = ['settings.all', ...unique];
        this.isLoading = false;
      },
      error: (err: unknown) => {
        console.error('Lỗi tải sản phẩm:', err);
        this.isLoading = false;
      },
    });
  }

  // ── Computed getters ─────────────────────────────────────────────
  get filteredProducts(): ProductCard[] {
    if (this.selectedCategory === 'settings.all') return this.featuredProducts;
    return this.featuredProducts.filter((p) => p.category === this.selectedCategory);
  }

  // ── Category filter ──────────────────────────────────────────────
  setCategory(category: string): void {
    this.selectedCategory = category;
  }

  // ── Comparison helpers ────────────────────────────────────────────
  isProductSelected(productId: string): boolean {
    return this.comparisonService.isSelected(productId);
  }

  toggleCompare(event: Event, product: ProductCard): void {
    event.stopPropagation();
    const cp: CompareProduct = {
      id: product.id,
      name: product.name,
      price: product.price,
      image: product.image,
      category: product.category,
      specs: product.specs,
    };
    this.comparisonService.toggleProduct(cp);
  }

  goToCompare(): void {
    this.router.navigate(['/product/comparison']);
  }
}
