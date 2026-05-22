import { FormsModule } from '@angular/forms';
import { Component, signal, computed, inject, OnInit, HostListener } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { catchError } from 'rxjs/operators';
import { of } from 'rxjs';

import { CartService } from '../../../core/services/cart.service';
import { ProductService } from '../../../core/services/product.service';
import { ProductCard, ProductFullDto } from '../../../core/models/product.model';
import { BrandService } from '../../../core/services/brand.service';
import { Brand } from '../../../core/models/brand.model';
import { ReviewService } from '../../../core/services/review.service';
import { ReviewDto } from '../../../core/models/review.model';
import { ToastService } from '../../../core/services/toast.service';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { WishlistToggleComponent } from '../../../shared/components/wishlist-toggle/wishlist-toggle';
import { VerifiedBadgeComponent } from '../../../shared/components/verified-badge/verified-badge';
import { AuthService } from '../../../core/services/auth.service';
import { CloudinaryService } from '../../../core/services/cloudinary.service';
import { FlashSaleService } from '../../../core/services/flash-sale.service';
import { firstValueFrom } from 'rxjs';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslatePipe,
    WishlistToggleComponent,
    VerifiedBadgeComponent,
  ],
  templateUrl: './product-detail.html',
})
export class ProductDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private cartService = inject(CartService);
  private productService = inject(ProductService);
  private reviewsService = inject(ReviewService);
  private brandService = inject(BrandService);
  private toastService = inject(ToastService);
  private authService = inject(AuthService);
  private cloudinaryService = inject(CloudinaryService);
  private flashSaleService = inject(FlashSaleService);

  activeTab = signal<string>('specs');
  activeImageIndex = signal<number>(0);
  isImageModalOpen = signal(false);
  isWriteReviewOpen = signal(false);
  selectedRating = signal(0);
  hoverRating = signal(0);
  reviewTitle = signal('');
  reviewBody = signal('');
  isSubmittingReview = signal(false);
  isLoading = signal(true);
  errorMsg = signal('');
  showStickyBar = signal(false);

  selectedImages = signal<{ file: File; preview: string }[]>([]);
  isReviewImageModalOpen = signal(false);
  activeReviewImageUrl = signal('');

  @HostListener('window:scroll', [])
  onWindowScroll() {
    this.showStickyBar.set(window.scrollY > 600);
  }

  // ── Product data ──────────────────────────────────────────────────
  // product() → UI model de dung trong template (gia, anh, ten...)
  product = signal<ProductCard>({
    id: '',
    name: '',
    slug: '',
    price: 0,
    regularPrice: 0,
    salePrice: null,
    isFlashSale: false,
    image: '',
    category: '',
    brand: '',
    brandId: '',
    stockQuantity: 0,
    warrantyMonths: 0,
    specs: {},
  });
  productImages = signal<string[]>([]);
  productSpecs = signal<{ specKey: string; specValue: string }[]>([]);
  keySpecs = computed(() => this.productSpecs().slice(0, 5));
  regularPrice = signal(0);
  salePrice = signal<number | null>(null);
  description = signal<string | null>(null);
  warrantyMonths = signal(0);

  productBrand = signal<Brand | null>(null);
  reviews = signal<ReviewDto[]>([]);

  averageRating = signal(0);
  totalReviews = signal(0);
  ratingDistribution = signal<{ stars: number; count: number; percentage: number }[]>([]);

  // ── Lifecycle ─────────────────────────────────────────────────────
  ngOnInit(): void {
    const slug = this.route.snapshot.paramMap.get('slug');
    if (!slug) {
      this.errorMsg.set('Khong tim thay san pham');
      this.isLoading.set(false);
      return;
    }
    this.loadProduct(slug);
  }

  // ── Data ──────────────────────────────────────────────────────────
  private loadProduct(slug: string): void {
    this.isLoading.set(true);
    this.errorMsg.set('');

    this.productService.getFullBySlug(slug).subscribe({
      next: (full: ProductFullDto) => {
        const dto = full.product;

        // Anh chinh: isPrimary hoac anh dau tien
        const primaryImg =
          full.images.find((i) => i.isPrimary)?.imageUrl || full.images[0]?.imageUrl || '';

        // Specs → Record de dung trong ProductCard
        const specsMap: Record<string, string> = {};
        const finalSpecs: { specKey: string; specValue: string }[] = [];

        if (dto.specifications) {
          try {
            const parsed = JSON.parse(dto.specifications);
            if (typeof parsed === 'object' && parsed !== null) {
              Object.entries(parsed).forEach(([key, value]) => {
                const valStr = String(value);
                finalSpecs.push({ specKey: key, specValue: valStr });
                specsMap[key] = valStr;
              });
            }
          } catch (e) {
            console.warn('Failed to parse specifications JSON', e);
          }
        }

        this.product.set({
          id: dto.productId,
          name: dto.name,
          slug: dto.slug,
          price: dto.salePrice ?? dto.regularPrice,
          regularPrice: dto.regularPrice,
          salePrice: dto.salePrice,
          isFlashSale: false,
          image: primaryImg,
          category: dto.categoryName,
          brand: '',
          brandId: dto.brandId,
          stockQuantity: dto.stockQuantity,
          warrantyMonths: dto.warrantyMonths,
          specs: specsMap,
        });

        // Kiểm tra nếu sản phẩm đang có Flash Sale → override giá
        this.flashSaleService.getActive().pipe(
          catchError(() => of(null))
        ).subscribe(flashSale => {
          if (!flashSale) return;
          const flashItem = flashSale.items.find(fi => fi.productId === dto.productId);
          if (flashItem && !flashItem.isSoldOut) {
            this.product.update(p => ({
              ...p,
              price: flashItem.flashPrice,
              regularPrice: flashItem.regularPrice,
              isFlashSale: true,
            }));
            // Cập nhật lại regularPrice signal để UI hiển thị đúng
            this.regularPrice.set(flashItem.regularPrice);
          }
        });

        if (dto.brandId) {
          this.brandService.getById(dto.brandId).subscribe({
            next: (brand) => {
              this.productBrand.set(brand);
              this.product.update((p) => ({ ...p, brand: brand.name }));
            },
            error: (err) => {
              console.warn('Lỗi tải thông tin thương hiệu:', err);
              this.productBrand.set(null);
            },
          });
        } else {
          this.productBrand.set(null);
        }

        this.productImages.set(full.images.map((i) => i.imageUrl));
        this.productSpecs.set(finalSpecs);
        this.regularPrice.set(dto.regularPrice);
        this.salePrice.set(dto.salePrice);
        this.description.set(dto.description);
        this.warrantyMonths.set(dto.warrantyMonths);
        this.activeImageIndex.set(0);
        this.isLoading.set(false);
        this.reviewsService.getByProductId(dto.productId).subscribe({
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
    this.reviewBody.set('');
    this.selectedImages.set([]);
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;

    const currentCount = this.selectedImages().length;
    const remaining = 3 - currentCount;
    const filesToAdd = Array.from(input.files).slice(0, remaining);

    for (const file of filesToAdd) {
      const error = this.cloudinaryService.validateImageFile(file, 5);
      if (error) {
        this.toastService.warning(error);
        continue;
      }
      const reader = new FileReader();
      reader.onload = (e) => {
        this.selectedImages.update((imgs) => [
          ...imgs,
          {
            file,
            preview: e.target?.result as string,
          },
        ]);
      };
      reader.readAsDataURL(file);
    }
    input.value = '';
  }

  removeImage(index: number) {
    this.selectedImages.update((imgs) => imgs.filter((_, i) => i !== index));
  }

  openReviewImage(url: string) {
    this.activeReviewImageUrl.set(url);
    this.isReviewImageModalOpen.set(true);
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

  buyNow(): void {
    this.cartService.setBuyNow(this.product());
    this.router.navigate(['/cart/checkout']);
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

  toggleHelpfulVote(review: ReviewDto, event: Event) {
    event.stopPropagation();

    const user = this.authService.currentUserValue;
    if (!user) {
      this.toastService.warning('Vui lòng đăng nhập để đánh giá hữu ích');
      return;
    }

    this.reviewsService.toggleVote(review.reviewId, { userId: user.userId }).subscribe({
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

  async submitReview() {
    const user = this.authService.currentUserValue;
    if (!user) {
      this.toastService.warning('Vui lòng đăng nhập để gửi đánh giá');
      return;
    }

    const rating = this.selectedRating();
    const comment = this.reviewBody().trim();

    if (rating === 0) {
      this.toastService.warning('Vui lòng chọn số sao đánh giá');
      return;
    }

    if (!comment) {
      this.toastService.warning('Vui lòng nhập nội dung đánh giá');
      return;
    }
    this.isSubmittingReview.set(true);

    const dto = {
      productId: this.product().id,
      userId: user.userId,
      rating: rating,
      comment: comment,
      isVerifiedPurchase: true,
    };

    try {
      const newReview = await firstValueFrom(this.reviewsService.createReview(dto));

      const imagesToUpload = this.selectedImages();
      if (imagesToUpload.length > 0) {
        for (const img of imagesToUpload) {
          try {
            const uploadRes = await firstValueFrom(
              this.cloudinaryService.uploadImage('reviews', img.file),
            );
            await firstValueFrom(
              this.reviewsService.addImage(newReview.reviewId, { imageUrl: uploadRes.imageUrl }),
            );
          } catch (err) {
            console.error('Lỗi upload ảnh review:', err);
            this.toastService.warning('Có lỗi tải ảnh lên, nhưng đánh giá đã được ghi nhận.');
          }
        }
        const fullReview = await firstValueFrom(this.reviewsService.getById(newReview.reviewId));
        this.reviews.update((reviews) => [fullReview, ...reviews]);
      } else {
        this.reviews.update((reviews) => [newReview, ...reviews]);
      }

      this.calculateRatingRate(this.reviews());
      this.toastService.success('Cảm ơn bạn đã gửi đánh giá!');
      this.toggleWriteReview();
    } catch (err: unknown) {
      console.error('Lỗi gửi đánh giá:', err);
      const apiError = err as { error?: { message?: string } };
      const errorMessage = apiError?.error?.message || 'Có lỗi xảy ra khi gửi đánh giá';
      this.toastService.error(errorMessage);
    } finally {
      this.isSubmittingReview.set(false);
    }
  }
}
