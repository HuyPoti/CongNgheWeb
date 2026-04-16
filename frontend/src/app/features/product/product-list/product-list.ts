import { Component, inject, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { RouterLink, Router, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  Subject,
  debounceTime,
  distinctUntilChanged,
  takeUntil,
  switchMap,
  tap,
  map,
  merge,
} from 'rxjs';

import { ComparisonService, CompareProduct } from '../../../core/services/comparison';
import { ProductCard, ProductListItemDto } from '../../../core/models/product.model';
import { ProductService } from '../../../core/services/product.service';
import { BrandService } from '../../../core/services/brand.service';
import { CategoryService } from '../../../core/services/category.service';
import { Brand } from '../../../core/models/brand.model';
import { Category } from '../../../core/models/category.model';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { ShopStateService, PriceRange } from '../../../core/services/shop-state.service';

@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [RouterLink, CommonModule, FormsModule, TranslatePipe],
  templateUrl: './product-list.html',
})
export class ProductList implements OnInit, OnDestroy {
  readonly comparisonService = inject(ComparisonService);
  readonly shopStateService = inject(ShopStateService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly productService = inject(ProductService);
  private readonly brandService = inject(BrandService);
  private readonly categoryService = inject(CategoryService);

  products = signal<ProductCard[]>([]);
  brands = signal<Brand[]>([]);
  categories = signal<Category[]>([]);
  currentCategorySlug = signal<string | null>(null);
  activeCategory: Category | null = null;
  isLoading = signal(true);
  hasError = signal(false);
  totalCount = signal(0);

  readonly pageSize = 12;

  private readonly destroy$ = new Subject<void>();
  private readonly searchInput$ = new Subject<string>();

  readonly priceRanges: PriceRange[] = [
    { labelKey: 'product.filter_price_under_10', max: 10_000_000 },
    { labelKey: 'product.filter_price_10_20', min: 10_000_000, max: 20_000_000 },
    { labelKey: 'product.filter_price_20_30', min: 20_000_000, max: 30_000_000 },
    { labelKey: 'product.filter_price_30_50', min: 30_000_000, max: 50_000_000 },
    { labelKey: 'product.filter_price_over_50', min: 50_000_000 },
  ];

  readonly sortOptions = [
    { value: 'newest', labelKey: 'product.sort_newest' },
    { value: 'price_asc', labelKey: 'product.sort_price_asc' },
    { value: 'price_desc', labelKey: 'product.sort_price_desc' },
  ];

  readonly stateSearchKeyword = this.shopStateService.searchKeyword;
  readonly stateBrandIds = this.shopStateService.selectedBrandIds;
  readonly statePriceRange = this.shopStateService.selectedPriceRange;
  readonly stateSort = this.shopStateService.selectedSort;
  readonly stateCurrentPage = this.shopStateService.currentPage;
  readonly stateHasActiveFilters = this.shopStateService.hasActiveFilters;

  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize)));

  readonly pageNumbers = computed(() => {
    const visible = 5;
    let start = Math.max(1, this.stateCurrentPage() - Math.floor(visible / 2));
    const end = Math.min(this.totalPages(), start + visible - 1);
    start = Math.max(1, end - visible + 1);
    return Array.from({ length: end - start + 1 }, (_, i) => start + i);
  });

  // ── Category tree ──────────────────────────────────────────────
  readonly rootCategories = computed(() => this.categories().filter((c) => c.parentId === null));

  readonly childrenByParentId = computed(() => {
    const grouped = new Map<string, Category[]>();

    for (const category of this.categories()) {
      if (!category.parentId) continue;

      const children = grouped.get(category.parentId) ?? [];
      children.push(category);
      grouped.set(category.parentId, children);
    }

    return grouped;
  });

  getChildren(parentId: string): Category[] {
    return this.childrenByParentId().get(parentId) ?? [];
  }

  onParentHover(category: Category): void {
    this.activeCategory = category;
  }

  onCategoryMenuLeave(): void {
    this.activeCategory = null;
  }

  isCategoryActive(slug: string): boolean {
    return this.currentCategorySlug() === slug;
  }

  isParentCategoryActive(parent: Category): boolean {
    const currentSlug = this.currentCategorySlug();
    if (!currentSlug) return false;
    if (parent.slug === currentSlug) return true;
    return this.getChildren(parent.categoryId).some((child) => child.slug === currentSlug);
  }

  selectCategory(slug: string | null): void {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { category: slug },
      queryParamsHandling: 'merge',
    });
  }
  // ──────────────────────────────────────────────────────────────

  ngOnInit(): void {
    this.loadBrands();
    this.loadCategories();

    // SEARCH
    this.searchInput$
      .pipe(debounceTime(400), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe((keyword) => {
        this.shopStateService.setSearchKeyword(keyword);
        this.shopStateService.setCurrentPage(1);
      });

    // CATEGORY từ URL
    const category$ = this.route.queryParams.pipe(
      takeUntil(this.destroy$),
      map((params) => (params['category'] as string | undefined) ?? null),
      distinctUntilChanged(),
      tap((category) => {
        this.currentCategorySlug.set(category);
        this.shopStateService.setCategorySlug(category);
        this.shopStateService.setCurrentPage(1);
      }),
    );

    // FILTER CHANGES
    const filters$ = this.shopStateService.filterChanged.pipe(takeUntil(this.destroy$));

    // MAIN DATA FLOW
    merge(category$, filters$)
      .pipe(
        takeUntil(this.destroy$),
        debounceTime(50),
        tap(() => {
          this.isLoading.set(true);
          this.hasError.set(false);
        }),
        switchMap(() => {
          const filters = this.shopStateService.getSnapshot();

          const brandId =
            filters.selectedBrandIds.size === 1 ? [...filters.selectedBrandIds][0] : undefined;

          return this.productService.fetchClientProducts({
            page: filters.currentPage,
            pageSize: this.pageSize,
            categorySlug: filters.categorySlug || undefined,
            keyword: filters.searchKeyword.trim() || undefined,
            brandId,
            minPrice: filters.selectedPriceRange?.min,
            maxPrice: filters.selectedPriceRange?.max,
            sortBy: filters.selectedSort as 'newest' | 'price_asc' | 'price_desc',
          });
        }),
      )
      .subscribe((res) => {
        this.totalCount.set(res.totalCount);
        this.products.set(res.items.map((p) => this.toCard(p)));
        this.isLoading.set(false);
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadBrands(): void {
    this.brandService.getAll().subscribe({
      next: (data) => this.brands.set(data.filter((b) => b.isActive)),
      error: () => this.brands.set([]),
    });
  }

  private loadCategories(): void {
    this.categoryService.getAll().subscribe({
      next: (data) => this.categories.set(data.filter((c) => c.isActive)),
      error: () => this.categories.set([]),
    });
  }

  onSearchInput(value: string): void {
    this.searchInput$.next(value);
  }

  clearSearch(): void {
    this.shopStateService.setSearchKeyword('');
    this.searchInput$.next('');
  }

  toggleBrand(brandId: string): void {
    this.shopStateService.toggleBrand(brandId);
  }

  isBrandSelected(brandId: string): boolean {
    return this.shopStateService.isBrandSelected(brandId);
  }

  togglePriceRange(range: PriceRange): void {
    this.shopStateService.togglePriceRange(range);
  }

  isPriceRangeSelected(range: PriceRange): boolean {
    return this.shopStateService.isPriceRangeSelected(range);
  }

  setSort(value: string): void {
    this.shopStateService.setSort(value);
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages() || page === this.stateCurrentPage()) return;
    this.shopStateService.setCurrentPage(page);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  clearFilters(): void {
    this.shopStateService.clearFilters();
  }

  private toCard(p: ProductListItemDto): ProductCard {
    return {
      id: p.id,
      name: p.name,
      price: p.price,
      regularPrice: p.regularPrice,
      salePrice: p.salePrice,
      image: p.thumbnailUrl ?? '',
      category: p.categoryName,
      brand: p.brandName,
      brandId: p.brandId,
      stockQuantity: p.stockQuantity,
      warrantyMonths: p.warrantyMonths,
      specs: {},
    };
  }

  hasSalePrice(p: ProductCard): boolean {
    return p.salePrice != null && p.salePrice < p.regularPrice;
  }

  savingPercent(p: ProductCard): number {
    if (!this.hasSalePrice(p) || p.regularPrice === 0) return 0;
    return Math.round(((p.regularPrice - p.price) / p.regularPrice) * 100);
  }

  isProductSelected(id: string): boolean {
    return this.comparisonService.isSelected(id);
  }

  toggleCompare(event: Event, p: ProductCard): void {
    event.preventDefault();
    event.stopPropagation();

    const cp: CompareProduct = {
      id: p.id,
      name: p.name,
      price: p.price,
      image: p.image,
      category: p.category,
      specs: p.specs,
    };

    this.comparisonService.toggleProduct(cp);
  }

  goToCompare(): void {
    this.router.navigate(['/comparison']);
  }
}
