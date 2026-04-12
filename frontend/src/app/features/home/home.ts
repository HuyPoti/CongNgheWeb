import { Component, inject, OnInit } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
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
  subtitle?: string;
  imageUrl: string;
  linkUrl?: string;
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
  private categoryService = inject(CategoryService);
  private mapToCard(p: ProductListItemDto): ProductCard {
    return {
      id: p.id,
      name: p.name,
      price: p.price,
      image: p.thumbnailUrl || 'path_to_default_img',
      category: p.categoryName,
      specs: {}
    };
  }
  // 2. Viết hàm tải sản phẩm theo Category
  // 2. Sửa lại hàm loadSection
  productSections: Record<'cpu' | 'gpu' | 'ram' | 'mainboard', ProductCard[]> = {
    cpu: [],
    gpu: [],
    ram: [],
    mainboard: []
  };
  private loadSection(catSlug: string, target: keyof typeof this.productSections) {
    this.productService.fetchClientProducts(1, 5, catSlug).subscribe({
      next: (res) => {
        this.productSections[target] = res.items.map(p => this.mapToCard(p));
      }
    });
  }
  comparisonService = inject(ComparisonService);

  // mainCategories = [
  //   { name: 'home.cat_laptop', icon: 'laptop_mac', slug: 'laptop' },
  //   { name: 'home.cat_pc', icon: 'desktop_windows', slug: 'pc-gaming' },
  //   { name: 'home.cat_components', icon: 'memory', slug: 'pc-components' },
  //   { name: 'home.cat_monitors', icon: 'monitor', slug: 'monitors' },
  //   { name: 'home.cat_keyboards', icon: 'keyboard', slug: 'keyboards' },
  //   { name: 'home.cat_mice', icon: 'mouse', slug: 'mice' },
  //   { name: 'home.cat_audio', icon: 'headset', slug: 'audio' },
  //   { name: 'home.cat_furniture', icon: 'chair', slug: 'gaming-furniture' },
  // ];

  banners: ClientBanner[] = [];
  featuredProducts: ProductCard[] = [];
  laptopGaming: ProductCard[] = [];
  pcGaming: ProductCard[] = [];
  monitors: ProductCard[] = [];
  accessories: ProductCard[] = [];
  isLoading = true;
  dbCategories: Category[] = [];
  isBannersLoading = true;

  ngOnInit(): void {
    this.loadBanners();
    this.loadFeaturedProducts();
    this.loadCategories();

    // Tải sản phẩm theo từng danh mục cụ thể
    this.loadSection('cpu', 'cpu');
    this.loadSection('gpu', 'gpu');
    this.loadSection('ram', 'ram');
    this.loadSection('mainboard', 'mainboard');
  }

  private loadCategories(): void {
    this.categoryService.getAll().subscribe({
      next: (data) => {
        this.dbCategories = data.filter(c => c.isActive && !c.parentId);
      },
      error: (err) => console.error('Lỗi khi fetch categories', err)
    })
  }

  private loadBanners(): void {
    this.isBannersLoading = true;
    this.bannerService.getPublic().subscribe({
      next: (banners: Banner[]) => {
        this.banners = banners
          .map((banner: Banner) => this.toClientBanner(banner))
          .sort((a: ClientBanner, b: ClientBanner) => a.position.localeCompare(b.position));
        this.isBannersLoading = false;
      },
      error: () => {
        this.banners = [];
        this.isBannersLoading = false;
      },
    });
  }

  private loadFeaturedProducts(): void {
    this.productService.fetchClientProducts(1, 20).subscribe({
      next: (res) => {
        this.featuredProducts = res.items.map((p: ProductListItemDto) => ({
          id: p.id,
          name: p.name,
          price: p.price,
          image: p.thumbnailUrl || 'https://ttgshop.vn/media/product/250_1072100124_dsc09857_copy.jpg',
          category: p.categoryName,
          specs: {},
        }));
        
        // Populate specific category arrays for the UI (using sliced copies of the fetched products)
        // In a real scenario, this would be fetched from specific category endpoints
        this.laptopGaming = this.featuredProducts.slice(0, 5);
        this.pcGaming = this.featuredProducts.slice(5, 10).length > 0 ? this.featuredProducts.slice(5, 10) : [...this.featuredProducts].reverse().slice(0, 5);
        this.monitors = this.featuredProducts.slice(10, 15).length > 0 ? this.featuredProducts.slice(10, 15) : this.featuredProducts.slice(0, 5);
        this.accessories = this.featuredProducts.slice(15, 20).length > 0 ? this.featuredProducts.slice(15, 20) : [...this.featuredProducts].reverse().slice(0, 5);
        
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Lỗi tải sản phẩm nổi bật:', err);
        this.isLoading = false;
      }
    });
  }

  get heroBanners(): ClientBanner[] {
    return this.banners.filter((b) => b.position === 'HOME PAGE HERO' && b.status === 'Live');
  }

  get heroBanner(): ClientBanner | undefined {
    return this.heroBanners[0];
  }

  get midBanners(): ClientBanner[] {
    return this.banners.filter((b) => b.position === 'SIDEBAR PROMO' && b.status === 'Live');
  }

  private toClientBanner(banner: Banner): ClientBanner {
    return {
      id: banner.bannerId,
      title: banner.title || 'Banner',
      subtitle: banner.subtitle || '',
      linkUrl: banner.linkUrl || '',
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
    this.router.navigate(['/comparison']);
  }
}
