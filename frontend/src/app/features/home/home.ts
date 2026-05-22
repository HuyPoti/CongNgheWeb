import { Component, inject, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { forkJoin, of } from 'rxjs'; // Thêm 'of'
import { catchError, finalize } from 'rxjs/operators'; // Thêm catchError và finalize
import { BannerService } from '../../core/services/banner.service';
import { Banner } from '../../core/models/banner.model';
import { ProductService } from '../../core/services/product.service';
import { FlashSaleService, FlashSaleDto, FlashSaleItemDto } from '../../core/services/flash-sale.service';
import { ProductCard, ProductListItemDto } from '../../core/models/product.model';
import { ComparisonService } from '../../core/services/comparison.service';
import { TranslatePipe } from '../../core/pipes/translate.pipe';
import { CategoryService } from '../../core/services/category.service';
import { Category } from '../../core/models/category.model';
import { BrandService } from '../../core/services/brand.service';
import { Brand } from '../../core/models/brand.model';
import { WishlistToggleComponent } from '../../shared/components/wishlist-toggle/wishlist-toggle';

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
  imports: [RouterLink, CommonModule, TranslatePipe, WishlistToggleComponent],
  templateUrl: './home.html',
})
export class HomeComponent implements OnInit, OnDestroy {
  private readonly bannerService = inject(BannerService);
  private readonly productService = inject(ProductService);
  private readonly categoryService = inject(CategoryService);
  private readonly brandService = inject(BrandService);
  private readonly router = inject(Router);
  private readonly cdr = inject(ChangeDetectorRef);
  readonly comparisonService = inject(ComparisonService);
  private readonly flashSaleService = inject(FlashSaleService);

  // ── State ────────────────────────────────────────────────────────
  banners: ClientBanner[] = [];
  activeFlashSale: FlashSaleDto | null = null;
  flashSaleProducts: ProductCard[] = [];
  flashSaleTimeLeft = { hours: '00', minutes: '00', seconds: '00' };
  featuredProducts: ProductCard[] = [];
  productSections: Record<'cpu' | 'gpu' | 'ram' | 'mainboard', ProductCard[]> = {
    cpu: [],
    gpu: [],
    ram: [],
    mainboard: [],
  };
  dbCategories: Category[] = [];
  dbBrands: Brand[] = [];
  isLoading = true;
  isBannersLoading = true;

  // Slider State
  currentSlide = 0;
  slideInterval: ReturnType<typeof setInterval> | undefined;

  // Flash Sale Timer
  private flashSaleInterval: ReturnType<typeof setInterval> | undefined;

  // ── Lifecycle ────────────────────────────────────────────────────
  ngOnInit(): void {
    // FIX NG0100: Sử dụng setTimeout để đợi Angular hoàn tất chu kỳ render/hydration hiện tại
    // rồi mới bắt đầu thay đổi các biến trạng thái (isLoading) và gọi API.
    setTimeout(() => {
      this.loadBanners();
      this.loadCategories();
      this.loadBrands();
      this.loadAllProducts();
    }, 0);
  }

  ngOnDestroy(): void {
    this.stopSlideTimer();
    this.stopFlashSaleTimer();
  }

  private stopFlashSaleTimer(): void {
    if (this.flashSaleInterval) {
      clearInterval(this.flashSaleInterval);
      this.flashSaleInterval = undefined;
    }
  }

  private startFlashSaleTimer(endTime: string): void {
    this.stopFlashSaleTimer();
    const targetMs = new Date(endTime).getTime();

    const tick = () => {
      const distance = targetMs - Date.now();
      if (distance <= 0) {
        this.stopFlashSaleTimer();
        this.activeFlashSale = null;
        this.flashSaleProducts = [];
        this.cdr.detectChanges();
        return;
      }
      const totalSec = Math.floor(distance / 1000);
      const hours   = Math.floor(totalSec / 3600);
      const minutes = Math.floor((totalSec % 3600) / 60);
      const seconds = totalSec % 60;
      this.flashSaleTimeLeft = {
        hours:   hours.toString().padStart(2, '0'),
        minutes: minutes.toString().padStart(2, '0'),
        seconds: seconds.toString().padStart(2, '0'),
      };
      this.cdr.detectChanges();
    };

    tick(); // render ngay lập tức
    this.flashSaleInterval = setInterval(tick, 1000);
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
          this.dbCategories = data.filter((c) => c.isActive && !c.parentId);
          this.cdr.markForCheck(); // Báo cho Angular biết state đã đổi
        },
      });
  }

  private loadBrands(): void {
    this.brandService
      .getAll()
      .pipe(
        catchError(() => of([])),
      )
      .subscribe({
        next: (data) => {
          this.dbBrands = data.filter((b) => b.isActive);
          this.cdr.markForCheck();
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
          this.startSlideTimer();
        },
        error: () => {
          this.banners = [];
          this.stopSlideTimer();
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
      flashSale: this.flashSaleService.getActive().pipe(catchError(() => of(null))),
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
          // ── Flash Sale ──────────────────────────────────────────────
          if (res.flashSale && res.flashSale.items?.length > 0) {
            this.activeFlashSale = res.flashSale;
            this.flashSaleProducts = res.flashSale.items.map((item: FlashSaleItemDto) => ({
              id: item.productId,
              slug: item.slug,
              name: item.productName,
              price: item.flashPrice,
              regularPrice: item.regularPrice,
              salePrice: item.flashPrice,
              image: item.thumbnailUrl ?? '',
              category: '',
              brand: '',
              brandId: '',
              stockQuantity: item.stockLimit - item.soldCount,
              warrantyMonths: 0,
              specs: {},
            } as ProductCard));
            this.startFlashSaleTimer(res.flashSale.endTime);
          }

          // ── Featured Products ───────────────────────────────────────
          this.featuredProducts = res.featured.items
            .map((p: ProductListItemDto) => this.toCard(p))
            .filter((p: ProductCard) => p.salePrice !== null && p.regularPrice - p.salePrice > 0)
            .slice(0, 4);
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
      slug: p.slug,
      name: p.name,
      price: p.price,
      regularPrice: p.regularPrice,
      salePrice: p.salePrice,
      isFlashSale: p.isFlashSale,
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
    return p.salePrice !== null && p.regularPrice > p.salePrice;
  }

  savingPercent(p: ProductCard): number {
    if (!p.salePrice || p.regularPrice <= p.salePrice) return 0;
    return Math.round(((p.regularPrice - p.salePrice) / p.regularPrice) * 100);
  }

  // ── Banner helpers ────────────────────────────────────────────────
  get heroBanners(): ClientBanner[] {
    return this.banners.filter((b) => b.position === 'HOME_HERO' && b.status === 'Live');
  }

  get midTopBanner(): ClientBanner | undefined {
    return this.banners.find((b) => b.position === 'HOME_MID_TR' && b.status === 'Live');
  }

  get midBottomBanner(): ClientBanner | undefined {
    return this.banners.find((b) => b.position === 'HOME_MID_BR' && b.status === 'Live');
  }

  get wideBanner(): ClientBanner | undefined {
    return this.banners.find((b) => b.position === 'HOME_MID_WIDE' && b.status === 'Live');
  }

  get rootCategories(): Category[] {
    return this.dbCategories.filter((c) => !c.parentId);
  }

  private toClientBanner(b: Banner): ClientBanner {
    let imageUrl = b.imageUrl;
    if (imageUrl && !imageUrl.startsWith('http')) {
      // Giả sử ảnh được lưu trong folder uploads của backend
      imageUrl = `http://localhost:5000/${imageUrl}`;
    }

    return {
      id: b.bannerId,
      title: b.title ?? '',
      subtitle: b.subtitle ?? '',
      imageUrl: imageUrl,
      linkUrl: b.linkUrl ?? '/product/list',
      targetAlt: b.subtitle ?? b.title ?? 'Banner',
      status: b.isActive ? 'Live' : 'Draft',
      position: this.mapPosition(b.position),
    };
  }

  // ── Slider Methods ────────────────────────────────────────────────
  nextSlide(): void {
    const total = this.heroBanners.length;
    if (total === 0) return;
    this.currentSlide = (this.currentSlide + 1) % total;
    this.cdr.markForCheck();
  }

  prevSlide(): void {
    const total = this.heroBanners.length;
    if (total === 0) return;
    this.currentSlide = (this.currentSlide - 1 + total) % total;
    this.cdr.markForCheck();
  }

  setSlide(index: number): void {
    this.currentSlide = index;
    this.cdr.markForCheck();
  }

  startSlideTimer(): void {
    this.stopSlideTimer();
    if (typeof window !== 'undefined') {
      this.slideInterval = setInterval(() => {
        this.nextSlide();
      }, 5000); // Tự động slide mỗi 5s
    }
  }

  stopSlideTimer(): void {
    if (this.slideInterval) {
      clearInterval(this.slideInterval);
    }
  }

  private mapPosition(pos: string | number): string {
    if (pos === null || pos === undefined) return '';
    const key = pos.toString().toLowerCase();
    const map: Record<string, string> = {
      homepage_slider: 'HOME_HERO',
      homepage_mid_top_right: 'HOME_MID_TR',
      homepage_mid_bottom_right: 'HOME_MID_BR',
      homepage_mid_wide: 'HOME_MID_WIDE',
      '0': 'HOME_HERO',
      '1': 'HOME_MID_TR',
      '2': 'HOME_MID_BR',
      '3': 'HOME_MID_WIDE',
    };
    return map[key] ?? key.toUpperCase();
  }

  // ── Comparison ────────────────────────────────────────────────────
  toggleCompare(event: Event, p: ProductCard): void {
    event.stopPropagation();
    this.comparisonService.toggleProductWithFetch(p.id, {
      name: p.name,
      price: p.price,
      image: p.image,
      category: p.category,
    });
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
