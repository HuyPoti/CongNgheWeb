import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../../core/services/toast.service';
import { ProductService } from '../../../core/services/product.service';
import { ProductDto, ProductFullDto } from '../../../core/models/product.model';
import { CategoryService } from '../../../core/services/category.service';
import { BrandService } from '../../../core/services/brand.service';

interface CategoryItem {
  categoryId: string;
  name: string;
}

interface BrandItem {
  brandId: string;
  name: string;
}

@Component({
  selector: 'app-emp-products',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './emp-products.html',
})
export class EmpProducts implements OnInit {
  private productService = inject(ProductService);
  private categoryService = inject(CategoryService);
  private brandService = inject(BrandService);
  private toast = inject(ToastService);

  // States
  isLoading = signal(true);
  products = signal<ProductDto[]>([]);
  categories = signal<CategoryItem[]>([]);
  brands = signal<BrandItem[]>([]);

  // Filters
  searchQuery = signal('');
  selectedCategoryId = signal<string | undefined>(undefined);
  currentPage = signal(1);
  pageSize = signal(20);
  totalCount = signal(0);

  // Detail view
  showDetail = signal<ProductFullDto | null>(null);
  isLoadingDetail = signal(false);

  // Stock status helpers
  stockThreshold = 5;

  ngOnInit() {
    this.loadProducts();
    this.loadCategories();
    this.loadBrands();
  }

  loadProducts() {
    this.isLoading.set(true);
    this.productService
      .getAll({
        keyword: this.searchQuery() || undefined,
        categoryId: this.selectedCategoryId(),
        page: this.currentPage(),
        pageSize: this.pageSize(),
      })
      .subscribe({
        next: (result) => {
          this.products.set(result.items);
          this.totalCount.set(result.totalCount);
          this.isLoading.set(false);
        },
        error: () => {
          this.toast.error('Không thể tải danh sách sản phẩm');
          this.isLoading.set(false);
        },
      });
  }

  loadCategories() {
    this.categoryService.getAll().subscribe({
      next: (cats) => this.categories.set(cats),
    });
  }

  loadBrands() {
    this.brandService.getAll().subscribe({
      next: (brands) => this.brands.set(brands),
    });
  }

  onSearch() {
    this.currentPage.set(1);
    this.loadProducts();
  }

  onCategoryChange(catId: string | undefined) {
    this.selectedCategoryId.set(catId);
    this.currentPage.set(1);
    this.loadProducts();
  }

  openDetail(product: ProductDto) {
    this.isLoadingDetail.set(true);
    this.productService.getFullById(product.productId).subscribe({
      next: (full) => {
        this.showDetail.set(full);
        this.isLoadingDetail.set(false);
      },
      error: () => {
        this.toast.error('Không thể tải chi tiết sản phẩm');
        this.isLoadingDetail.set(false);
      },
    });
  }

  closeDetail() {
    this.showDetail.set(null);
  }

  getStockStatus(qty: number): { label: string; color: string; icon: string } {
    if (qty <= 0) return { label: 'Hết hàng', color: 'text-red-400 bg-red-500/10', icon: 'error' };
    if (qty <= this.stockThreshold)
      return { label: 'Sắp hết', color: 'text-yellow-400 bg-yellow-500/10', icon: 'warning' };
    return { label: 'Còn hàng', color: 'text-emerald-400 bg-emerald-500/10', icon: 'check_circle' };
  }

  getPrice(product: ProductDto): number {
    return product.salePrice ?? product.regularPrice;
  }

  getDiscount(product: ProductDto): number | null {
    if (!product.salePrice || product.salePrice >= product.regularPrice) return null;
    return Math.round((1 - product.salePrice / product.regularPrice) * 100);
  }

  getBrandName(brandId: string): string {
    return this.brands().find((b) => b.brandId === brandId)?.name || '—';
  }

  getProductStats() {
    const prods = this.products();
    return {
      total: this.totalCount(),
      outOfStock: prods.filter((p) => p.stockQuantity <= 0).length,
      lowStock: prods.filter((p) => p.stockQuantity > 0 && p.stockQuantity <= this.stockThreshold)
        .length,
      inStock: prods.filter((p) => p.stockQuantity > this.stockThreshold).length,
    };
  }
}
