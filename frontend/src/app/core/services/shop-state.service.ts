import { Injectable, signal, computed } from '@angular/core';
import { Subject } from 'rxjs';

export interface PriceRange {
  labelKey: string;
  min?: number;
  max?: number;
}

export interface ShopFilters {
  searchKeyword: string;
  selectedBrandIds: Set<string>;
  selectedPriceRange: PriceRange | null;
  selectedSort: string;
  currentPage: number;
  categorySlug: string | null;
}

export interface ShopState {
  filters: ShopFilters;
}

@Injectable({ providedIn: 'root' })
export class ShopStateService {
  // ── Signals ──────────────────────────────────────────────
  private readonly state = signal<ShopState>({
    filters: {
      searchKeyword: '',
      selectedBrandIds: new Set<string>(),
      selectedPriceRange: null,
      selectedSort: 'newest',
      currentPage: 1,
      categorySlug: null,
    },
  });

  // ── Public readonly signals ──────────────────────────────
  readonly filters = computed(() => this.state().filters);
  readonly searchKeyword = computed(() => this.state().filters.searchKeyword);
  readonly selectedBrandIds = computed(() => this.state().filters.selectedBrandIds);
  readonly selectedPriceRange = computed(() => this.state().filters.selectedPriceRange);
  readonly selectedSort = computed(() => this.state().filters.selectedSort);
  readonly currentPage = computed(() => this.state().filters.currentPage);
  readonly categorySlug = computed(() => this.state().filters.categorySlug);

  // ── Computed: check if has active filters ──────────────
  readonly hasActiveFilters = computed(() => {
    const filters = this.filters();
    return !!(
      filters.searchKeyword.trim() ||
      filters.selectedBrandIds.size > 0 ||
      filters.selectedPriceRange
    );
  });

  // ── Subject để notify khi cần reload ────────────────────
  private readonly filterChanged$ = new Subject<void>();
  readonly filterChanged = this.filterChanged$.asObservable();

  // ── Methods ──────────────────────────────────────────────

  setSearchKeyword(keyword: string): void {
    this.state.update((s) => ({
      ...s,
      filters: { ...s.filters, searchKeyword: keyword },
    }));
    this.filterChanged$.next();
  }

  toggleBrand(brandId: string): void {
    this.state.update((s) => {
      const newBrandIds = new Set(s.filters.selectedBrandIds);
      if (newBrandIds.has(brandId)) {
        newBrandIds.delete(brandId);
      } else {
        newBrandIds.add(brandId);
      }
      return {
        ...s,
        filters: {
          ...s.filters,
          selectedBrandIds: newBrandIds,
          currentPage: 1, // Reset to page 1
        },
      };
    });
    this.filterChanged$.next();
  }

  isBrandSelected(brandId: string): boolean {
    return this.selectedBrandIds().has(brandId);
  }

  togglePriceRange(range: PriceRange): void {
    this.state.update((s) => {
      const isSelected = s.filters.selectedPriceRange?.labelKey === range.labelKey;
      return {
        ...s,
        filters: {
          ...s.filters,
          selectedPriceRange: isSelected ? null : range,
          currentPage: 1, // Reset to page 1
        },
      };
    });
    this.filterChanged$.next();
  }

  isPriceRangeSelected(range: PriceRange): boolean {
    return this.selectedPriceRange()?.labelKey === range.labelKey;
  }

  setSort(value: string): void {
    if (this.selectedSort() === value) return;
    this.state.update((s) => ({
      ...s,
      filters: {
        ...s.filters,
        selectedSort: value,
        currentPage: 1, // Reset to page 1
      },
    }));
    this.filterChanged$.next();
  }

  setCurrentPage(page: number): void {
    this.state.update((s) => ({
      ...s,
      filters: {
        ...s.filters,
        currentPage: page,
      },
    }));
    this.filterChanged$.next();
  }

  setCategorySlug(slug: string | null): void {
    this.state.update((s) => ({
      ...s,
      filters: {
        ...s.filters,
        categorySlug: slug,
        currentPage: 1, // Reset to page 1
      },
    }));
  }

  clearFilters(): void {
    this.state.set({
      filters: {
        searchKeyword: '',
        selectedBrandIds: new Set<string>(),
        selectedPriceRange: null,
        selectedSort: 'newest',
        currentPage: 1,
        categorySlug: null,
      },
    });
    this.filterChanged$.next();
  }

  restoreState(filters: Partial<ShopFilters>): void {
    this.state.update((s) => ({
      ...s,
      filters: {
        ...s.filters,
        ...filters,
        // Đảm bảo Set được khôi phục đúng
        selectedBrandIds: filters.selectedBrandIds || new Set<string>(),
      },
    }));
  }

  getSnapshot(): ShopFilters {
    return this.filters();
  }
}
