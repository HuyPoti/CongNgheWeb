import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { forkJoin, of } from 'rxjs'; // Thêm 'of'
import { catchError, finalize } from 'rxjs/operators'; // Thêm catchError và finalize
import { BannerService } from '../../core/services/banner.service';
import { Banner } from '../../core/models/banner.model';
import { ProductService } from '../../core/services/product.service';
import { ProductCard, ProductListItemDto } from '../../core/models/product.model';
import { ComparisonService, CompareProduct } from '../../core/services/comparison';
import { TranslatePipe } from '../../core/pipes/translate.pipe';
import { CategoryService } from '../../core/services/category.service';
import { Category } from '../../core/models/category.model';

export interface ClientBanner {
  id: string;
  title: string;
  subtitle: string;
  imageUrl: string;
  linkUrl: string;
  targetAlt: string;
  status: 'Live' | 'Draft';
  position: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink, CommonModule, TranslatePipe],
  templateUrl: './home.html',
})
export class HomeComponent implements OnInit {
  private readonly bannerService = inject(BannerService);
  private readonly productService = inject(ProductService);
  private readonly categoryService = inject(CategoryService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);
  readonly comparisonService = inject(ComparisonService);

  // ── State ────────────────────────────────────────────────────────
  banners: ClientBanner[] = [];
  featuredProducts: ProductCard[] = [];
  productSections: Record<'cpu' | 'gpu' | 'ram' | 'mainboard', ProductCard[]> = {
    cpu: [],
    gpu: [],
    ram: [],
    mainboard: [],
  };
  dbCategories: Category[] = [];
  isLoading = true;
  isBannersLoading = true;
  bannerImageError: Record<string, boolean> = {};

  // ── Lifecycle ────────────────────────────────────────────────────
  ngOnInit(): void {
    // FIX NG0100: Sử dụng setTimeout để đợi Angular hoàn tất chu kỳ render/hydration hiện tại
    // rồi mới bắt đầu thay đổi các biến trạng thái (isLoading) và gọi API.
    setTimeout(() => {
      this.loadBanners();
      this.loadCategories();
      this.loadAllProducts();
    }, 0);
  }

  // ── Loaders ───────────────────────────────────────────────────────
  private loadCategories(): void {
    this.categoryService
      .getAll()
      .pipe(
        catchError(() => of([])), // Nếu lỗi, trả về mảng rỗng để không crash app
      )
      .subscribe({
        next: (data) => {
          this.dbCategories = data.filter((c) => c.isActive);
          this.cdr.markForCheck(); // Báo cho Angular biết state đã đổi
        },
      });
  }

  private loadBanners(): void {
    this.isBannersLoading = true;
    this.cdr.markForCheck();

    this.bannerService
      .getPublic()
      .pipe(
        finalize(() => {
          this.isBannersLoading = false;
          this.cdr.detectChanges(); // Ép giao diện cập nhật ngay lập tức
        }),
      )
      .subscribe({
        next: (banners) => {
          this.banners = banners.map((b) => this.toClientBanner(b));
        },
        error: () => {
          this.banners = [];
        },
      });
  }

  private loadAllProducts(): void {
    this.isLoading = true;
    this.cdr.markForCheck();

    // Chuẩn bị fallback data khi gọi API thất bại
    const fallback = { items: [] };

    forkJoin({
      // FIX LỖI MẤT DỮ LIỆU: Thêm catchError cho từng request
      featured: this.productService
        .fetchClientProducts({ page: 1, pageSize: 20 })
        .pipe(catchError(() => of(fallback))),
      cpu: this.productService
        .fetchClientProducts({ page: 1, pageSize: 5, categorySlug: 'cpu' })
        .pipe(catchError(() => of(fallback))),
      gpu: this.productService
        .fetchClientProducts({ page: 1, pageSize: 5, categorySlug: 'gpu' })
        .pipe(catchError(() => of(fallback))),
      ram: this.productService
        .fetchClientProducts({ page: 1, pageSize: 5, categorySlug: 'ram' })
        .pipe(catchError(() => of(fallback))),
      mainboard: this.productService
        .fetchClientProducts({
          page: 1,
          pageSize: 5,
          categorySlug: 'mainboard',
        })
        .pipe(catchError(() => of(fallback))),
    })
      .pipe(
        finalize(() => {
          // finalize luôn chạy cho dù API thành công hay thất bại
          this.isLoading = false;
          // FIX LỖI BACK TRANG: Ép Angular vẽ lại giao diện ngay khi có dữ liệu từ Cache hoặc API
          this.cdr.detectChanges();
        }),
      )
      .subscribe({
        next: (res) => {
          this.featuredProducts = res.featured.items.map((p: ProductListItemDto) => this.toCard(p));
          this.productSections.cpu = res.cpu.items.map((p: ProductListItemDto) => this.toCard(p));
          this.productSections.gpu = res.gpu.items.map((p: ProductListItemDto) => this.toCard(p));
          this.productSections.ram = res.ram.items.map((p: ProductListItemDto) => this.toCard(p));
          this.productSections.mainboard = res.mainboard.items.map((p: ProductListItemDto) =>
            this.toCard(p),
          );
        },
      });
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

  // ── Banner helpers ────────────────────────────────────────────────
  get heroBanner(): ClientBanner | undefined {
    return this.banners.find((b) => b.position === 'HOME_HERO' && b.status === 'Live');
  }

  get midBanners(): ClientBanner[] {
    return this.banners.filter((b) => b.position === 'HOME_MID' && b.status === 'Live');
  }

  get rootCategories(): Category[] {
    return this.dbCategories.filter((c) => !c.parentId);
  }

  private toClientBanner(b: Banner): ClientBanner {
    return {
      id: b.bannerId,
      title: b.title ?? '',
      subtitle: b.subtitle ?? '',
      imageUrl: b.imageUrl,
      linkUrl: b.linkUrl ?? '/product/list',
      targetAlt: b.subtitle ?? b.title ?? 'Banner',
      status: b.isActive ? 'Live' : 'Draft',
      position: this.mapPosition(b.position),
    };
  }

  private mapPosition(pos: Banner['position']): string {
    const map: Record<string, string> = {
      homepage_slider: 'HOME_HERO',
      homepage_mid: 'HOME_MID',
      category_top: 'CAT_TOP',
      news_top: 'NEWS_TOP',
    };
    return map[pos] ?? pos;
  }

  handleBannerError(id: string): void {
    this.bannerImageError[id] = true;
    this.cdr.markForCheck();
  }

  // ── Comparison ────────────────────────────────────────────────────
  toggleCompare(event: Event, p: ProductCard): void {
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

  isProductSelected(id: string): boolean {
    return this.comparisonService.isSelected(id);
  }

  goToCompare(): void {
    this.router.navigate(['/comparison']);
  }

  getSubCategoriesBySlug(parentSlug: string): Category[] {
    const parent = this.dbCategories.find((c) => c.slug === parentSlug);
    if (!parent) return [];
    return this.dbCategories.filter((c) => c.parentId === parent.categoryId);
  }

  goToCategory(slug: string) {
    this.router.navigate(['/product/list'], {
      queryParams: { category: slug },
    });
  }
}
