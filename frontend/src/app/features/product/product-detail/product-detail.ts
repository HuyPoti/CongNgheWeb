import { Component, signal, inject, OnInit } from '@angular/core';
import { RouterLink, ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { CartService } from '../../../core/services/cart.service';
import { ProductService } from '../../../core/services/product.service';
import {
  ProductCard,
  Review,
  ProductFullDto,
  ProductSpecDto,
} from '../../../core/models/product.model';
import { MOCK_REVIEWS } from '../../../core/mocks/product.mock';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [RouterLink, CommonModule, TranslatePipe],
  templateUrl: './product-detail.html',
  styles: ``,
})
export class ProductDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private cartService = inject(CartService);
  private productService = inject(ProductService);

  activeTab = signal<string>('specs');
  isWriteReviewOpen = signal(false);
  selectedRating = signal(0);
  hoverRating = signal(0);
  isLoading = signal(true);
  errorMsg = signal('');

  // product() được dùng bởi HTML - giữ kiểu ProductCard để template không đổi
  product = signal<ProductCard>({
    id: '',
    name: 'Đang tải...',
    price: 0,
    image: '',
    category: '',
    specs: {},
    brand: '',
  });

  // Dữ liệu thêm từ API
  productFull = signal<ProductFullDto | null>(null);
  productImages = signal<string[]>([]);
  productSpecs = signal<ProductSpecDto[]>([]);
  regularPrice = signal(0);
  salePrice = signal<number | null>(null);
  description = signal<string | null>(null);
  warrantyMonths = signal(0);

  reviews = signal<Review[]>(MOCK_REVIEWS);

  averageRating = 4.7;
  totalReviews = 128;
  ratingDistribution = [
    { stars: 5, count: 89, percentage: 70 },
    { stars: 4, count: 25, percentage: 20 },
    { stars: 3, count: 8, percentage: 6 },
    { stars: 2, count: 4, percentage: 3 },
    { stars: 1, count: 2, percentage: 1 },
  ];

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.errorMsg.set('Không tìm thấy sản phẩm');
      this.isLoading.set(false);
      return;
    }

    this.productService.getFullById(id).subscribe({
      next: (full: ProductFullDto) => {
        const dto = full.product;

        // Lấy ảnh chính (isPrimary = true hoặc ảnh đầu tiên)
        const primaryImg =
          full.images.find((i) => i.isPrimary)?.imageUrl ||
          full.images[0]?.imageUrl ||
          'https://via.placeholder.com/600x600?text=No+Image';

        // Map specs thành Record<string, string> cho ProductCard
        const specsMap: Record<string, string> = {};
        full.specs.forEach((s) => (specsMap[s.specKey] = s.specValue));

        this.product.set({
          id: dto.productId,
          name: dto.name,
          price: dto.salePrice ?? dto.regularPrice,
          image: primaryImg,
          category: dto.categoryName,
          specs: specsMap,
          brand: '',
        });

        this.productFull.set(full);
        this.productImages.set(full.images.map((i) => i.imageUrl));
        this.productSpecs.set(full.specs);
        this.regularPrice.set(dto.regularPrice);
        this.salePrice.set(dto.salePrice);
        this.description.set(dto.description);
        this.warrantyMonths.set(dto.warrantyMonths);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Lỗi tải sản phẩm:', err);
        this.errorMsg.set('Không thể tải thông tin sản phẩm');
        this.isLoading.set(false);
      },
    });
  }

  setTab(tab: string) {
    this.activeTab.set(tab);
  }

  getStarArray(rating: number): boolean[] {
    return Array.from({ length: 5 }, (_, i) => i < rating);
  }

  toggleWriteReview() {
    this.isWriteReviewOpen.update((v) => !v);
    this.selectedRating.set(0);
    this.hoverRating.set(0);
  }

  setRating(rating: number) {
    this.selectedRating.set(rating);
  }

  setHoverRating(rating: number) {
    this.hoverRating.set(rating);
  }

  addToCart() {
    this.cartService.addToCart(this.product());
  }
}
