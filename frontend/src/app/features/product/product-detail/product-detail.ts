import { FormsModule } from '@angular/forms';
import { Component, signal, inject, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';

import { CartService } from '../../../core/services/cart.service';
import { ProductService } from '../../../core/services/product.service';
import { ProductCard, ProductFullDto, ProductSpecDto } from '../../../core/models/product.model';
import { ReviewService } from '../../../core/services/review.service';
import { ReviewDto } from '../../../core/models/review.model';
import { ToastService } from '../../../core/services/toast.service';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './product-detail.html',
})
export class ProductDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private cartService = inject(CartService);
  private productService = inject(ProductService);
  private reviewsService = inject(ReviewService);
  private toastService = inject(ToastService);

  activeTab = signal<string>('specs');
  isWriteReviewOpen = signal(false);
  selectedRating = signal(0);
  hoverRating = signal(0);
  reviewTitle = signal('');
  reviewBody = signal('');
  isSubmittingReview = signal(false);
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
  activeImageIndex = signal(0);
  productImages = signal<string[]>([]);
  productSpecs = signal<ProductSpecDto[]>([]);
  regularPrice = signal(0);
  salePrice = signal<number | null>(null);
  description = signal<string | null>(null);
  warrantyMonths = signal(0);

  reviews = signal<ReviewDto[]>([]);

  averageRating = signal(0);
  totalReviews = signal(0);
  ratingDistribution = signal<{ stars: number; count: number; percentage: number }[]>([]);

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
        this.reviewsService.getByProductId(id).subscribe({
          next: (data) => {
            this.reviews.set(data);
            this.calculateRatingRate(data);
          },
          error: (err) => {
            console.error('Lỗi tải reviews: ', err);
            this.reviews.set([]);
          },
        });
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

  private calculateRatingRate(reviews: ReviewDto[]) {
    if (reviews.length === 0) {
      this.averageRating.set(0);
      this.totalReviews.set(0);
      this.ratingDistribution.set([]);
      return;
    }

    const total = reviews.length;
    const sum = reviews.reduce((acc, r) => acc + r.rating, 0);
    const avg = Math.floor((sum / total) * 10) / 10;

    const distribution: { stars: number; count: number; percentage: number }[] = [];
    for (let stars = 5; stars >= 1; stars--) {
      const count = reviews.filter((r) => r.rating === stars).length;
      distribution.push({
        stars,
        count,
        percentage: Math.round((count / total) * 100),
      });
    }
    this.averageRating.set(avg);
    this.totalReviews.set(total);
    this.ratingDistribution.set(distribution);
  }

  private readonly DEMO_USER_ID = '22222222-2222-2222-2222-222222222222';
  toggleHelpfulVote(review: ReviewDto, event: Event) {
    event.stopPropagation();

    this.reviewsService.toggleVote(review.reviewId, { userId: this.DEMO_USER_ID }).subscribe({
      next: (response) => {
        // Cập nhật lại review trong list
        const updatedReviews = this.reviews().map((r) => {
          if (r.reviewId === review.reviewId) {
            return { ...r, helpfulVoteCount: response.helpfulCount };
          }
          return r;
        });
        this.reviews.set(updatedReviews);
      },
      error: (err) => {
        console.error('Lỗi toggle vote:', err);
      },
    });
  }

  submitReview() {
    const rating = this.selectedRating();
    const comment = this.reviewBody().trim();

    if (rating === 0) {
      alert('Vui lòng chọn số sao đánh giá');
      return;
    }

    if (!comment) {
      alert('Vui lòng nhập nội dung đánh giá');
      return;
    }
    this.isSubmittingReview.set(true);

    const dto = {
      productId: this.product().id,
      userId: this.DEMO_USER_ID,
      rating: rating,
      comment: comment,
      isVerifiedPurchase: true,
    };
    this.reviewsService.createReview(dto).subscribe({
      next: (newReview) => {
        this.reviews.update((reviews) => [newReview, ...reviews]);
        this.calculateRatingRate(this.reviews());
        this.reviewTitle.set('');
        this.reviewBody.set('');
        this.selectedRating.set(0);
        this.isWriteReviewOpen.set(false);
        this.isSubmittingReview.set(false);
      },
      error: (err) => {
        console.error('Lỗi gửi đánh giá:', err);
        this.isSubmittingReview.set(false);
      },
    });
  }
}
