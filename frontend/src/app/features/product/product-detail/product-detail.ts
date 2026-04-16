import { Component, signal, inject, OnInit } from '@angular/core';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { CartService } from '../../../core/services/cart.service';
import { ToastService } from '../../../core/services/toast.service';
import { ProductService } from '../../../core/services/product.service';
import { ProductCard, ProductFullDto, ProductSpecDto } from '../../../core/models/product.model';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [RouterLink, CommonModule, TranslatePipe],
  templateUrl: './product-detail.html',
})
export class ProductDetail implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly cartService = inject(CartService);
  private readonly productService = inject(ProductService);
  private readonly toastService = inject(ToastService);

  // ── Loading / error state ─────────────────────────────────────────
  isLoading = signal(true);
  errorMsg = signal('');

  // ── Product data ──────────────────────────────────────────────────
  // product() → UI model de dung trong template (gia, anh, ten...)
  product = signal<ProductCard>({
    id: '',
    name: '',
    price: 0,
    regularPrice: 0,
    salePrice: null,
    image: '',
    category: '',
    brand: '',
    brandId: '',
    stockQuantity: 0,
    warrantyMonths: 0,
    specs: {},
  });

  productImages = signal<string[]>([]);
  productSpecs = signal<ProductSpecDto[]>([]);
  regularPrice = signal(0);
  salePrice = signal<number | null>(null);
  description = signal<string | null>(null);
  warrantyMonths = signal(0);

  // Anh dang hien thi trong main image viewer
  activeImageIndex = signal(0);

  // ── Tab / review modal state ──────────────────────────────────────
  activeTab = signal<string>('description');
  isWriteReviewOpen = signal(false);
  selectedRating = signal(0);
  hoverRating = signal(0);

  // Rating tong hop (tinh tu du lieu thuc neu co, hien tai placeholder)
  averageRating = 0;
  totalReviews = 0;
  ratingDistribution: { stars: number; percentage: number }[] = [
    { stars: 5, percentage: 0 },
    { stars: 4, percentage: 0 },
    { stars: 3, percentage: 0 },
    { stars: 2, percentage: 0 },
    { stars: 1, percentage: 0 },
  ];

  // ── Lifecycle ─────────────────────────────────────────────────────
  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.errorMsg.set('Khong tim thay san pham');
      this.isLoading.set(false);
      return;
    }
    this.loadProduct(id);
  }

  // ── Data ──────────────────────────────────────────────────────────
  private loadProduct(id: string): void {
    this.isLoading.set(true);
    this.errorMsg.set('');

    this.productService.getFullById(id).subscribe({
      next: (full: ProductFullDto) => {
        const dto = full.product;

        // Anh chinh: isPrimary hoac anh dau tien
        const primaryImg =
          full.images.find((i) => i.isPrimary)?.imageUrl || full.images[0]?.imageUrl || '';

        // Specs → Record de dung trong ProductCard
        const specsMap: Record<string, string> = {};
        full.specs.forEach((s) => (specsMap[s.specKey] = s.specValue));

        this.product.set({
          id: dto.productId,
          name: dto.name,
          price: dto.salePrice ?? dto.regularPrice,
          regularPrice: dto.regularPrice,
          salePrice: dto.salePrice,
          image: primaryImg,
          category: dto.categoryName,
          brand: '',
          brandId: dto.brandId,
          stockQuantity: dto.stockQuantity,
          warrantyMonths: dto.warrantyMonths,
          specs: specsMap,
        });

        this.productImages.set(full.images.map((i) => i.imageUrl));
        this.productSpecs.set(full.specs);
        this.regularPrice.set(dto.regularPrice);
        this.salePrice.set(dto.salePrice);
        this.description.set(dto.description);
        this.warrantyMonths.set(dto.warrantyMonths);
        this.activeImageIndex.set(0);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMsg.set('Khong the tai thong tin san pham. Vui long thu lai.');
        this.isLoading.set(false);
      },
    });
  }

  // ── Tab ───────────────────────────────────────────────────────────
  setTab(tab: string): void {
    this.activeTab.set(tab);
  }

  // ── Image gallery ─────────────────────────────────────────────────
  setActiveImage(index: number): void {
    this.activeImageIndex.set(index);
  }

  get mainImageUrl(): string {
    const imgs = this.productImages();
    return imgs[this.activeImageIndex()] ?? this.product().image;
  }

  // ── Review modal ──────────────────────────────────────────────────
  toggleWriteReview(): void {
    this.isWriteReviewOpen.update((v) => !v);
    this.selectedRating.set(0);
    this.hoverRating.set(0);
  }

  setRating(rating: number): void {
    this.selectedRating.set(rating);
  }
  setHoverRating(rating: number): void {
    this.hoverRating.set(rating);
  }

  getStarArray(rating: number): boolean[] {
    return Array.from({ length: 5 }, (_, i) => i < Math.round(rating));
  }

  // ── Cart ──────────────────────────────────────────────────────────
  addToCart(): void {
    this.cartService.addToCart(this.product());
    this.toastService.success(`Da them "${this.product().name}" vao gio hang`);
  }
}
