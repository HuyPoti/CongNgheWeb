import { Component, inject, OnInit } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { BannerService } from '../../core/services/banner.service';
import { Banner } from '../../core/models/banner.model';
import { ProductService } from '../../core/services/product.service';
import { ProductCard, ProductListItemDto } from '../../core/models/product.model';
import { ComparisonService, CompareProduct } from '../../core/services/comparison';
import { TranslatePipe } from '../../core/pipes/translate.pipe';

export interface ClientBanner {
  id: string;
  title: string;
  imageUrl: string;
  targetAlt: string;
  status: 'Live' | 'Draft';
  position: string;
  lastUpdated: string;
  author: string;
}

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [RouterLink, CommonModule, TranslatePipe],
  templateUrl: './home.html',
  styles: ``,
})
export class HomeComponent implements OnInit {
  private bannerService = inject(BannerService);
  private productService = inject(ProductService);
  private router = inject(Router);
  comparisonService = inject(ComparisonService);

  mainCategories = [
    { name: 'home.cat_laptop', icon: 'laptop_mac', slug: 'laptop' },
    { name: 'home.cat_pc', icon: 'desktop_windows', slug: 'pc-gaming' },
    { name: 'home.cat_components', icon: 'memory', slug: 'pc-components' },
    { name: 'home.cat_monitors', icon: 'monitor', slug: 'monitors' },
    { name: 'home.cat_keyboards', icon: 'keyboard', slug: 'keyboards' },
    { name: 'home.cat_mice', icon: 'mouse', slug: 'mice' },
    { name: 'home.cat_audio', icon: 'headset', slug: 'audio' },
    { name: 'home.cat_furniture', icon: 'chair', slug: 'gaming-furniture' },
  ];

  banners: ClientBanner[] = [];
  featuredProducts: ProductCard[] = [];
  isLoading = true;

  ngOnInit(): void {
    this.loadBanners();
    this.loadFeaturedProducts();
  }

  private loadBanners(): void {
    this.bannerService.getPublic().subscribe({
      next: (banners: Banner[]) => {
        this.banners = banners
          .map((banner: Banner) => this.toClientBanner(banner))
          .sort((a: ClientBanner, b: ClientBanner) => a.position.localeCompare(b.position));
      },
      error: () => {
        this.banners = [];
      },
    });
  }

  private loadFeaturedProducts(): void {
    this.productService.fetchClientProducts(1, 8).subscribe({
      next: (res) => {
        this.featuredProducts = res.items.map((p: ProductListItemDto) => ({
          id: p.id,
          name: p.name,
          price: p.price,
          image: p.thumbnailUrl || 'https://ttgshop.vn/media/product/250_1072100124_dsc09857_copy.jpg',
          category: p.categoryName,
          specs: {},
        }));
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Lỗi tải sản phẩm nổi bật:', err);
        this.isLoading = false;
      }
    });
  }

  get heroBanner(): ClientBanner | undefined {
    return this.banners.find((b) => b.position === 'HOME PAGE HERO' && b.status === 'Live');
  }

  get promoBanner(): ClientBanner | undefined {
    return this.banners.find((b) => b.position === 'SIDEBAR PROMO' && b.status === 'Live');
  }

  private toClientBanner(banner: Banner): ClientBanner {
    return {
      id: banner.bannerId,
      title: banner.title || 'Banner',
      imageUrl: banner.imageUrl,
      targetAlt: banner.subtitle || banner.title || 'Banner image',
      status: banner.isActive ? 'Live' : 'Draft',
      position: this.mapPositionToClientLabel(banner.position),
      lastUpdated: 'System',
      author: 'System',
    };
  }

  private mapPositionToClientLabel(position: Banner['position']): string {
    switch (position) {
      case 'homepage_slider': return 'HOME PAGE HERO';
      case 'homepage_mid': return 'SIDEBAR PROMO';
      case 'category_top': return 'CATEGORY TOP';
      case 'news_top': return 'NEWS TOP';
      default: return position;
    }
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

  isProductSelected(productId: string): boolean {
    return this.comparisonService.isSelected(productId);
  }

  goToCompare(): void {
    this.router.navigate(['/product/comparison']);
  }
}
