import { Component, inject, OnInit, ChangeDetectorRef, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { WishlistService, WishlistItem } from '../../../core/services/wishlist.service';
import { CartService } from '../../../core/services/cart.service';
import { ToastService } from '../../../core/services/toast.service';
import { ProductCard } from '../../../core/models/product.model';
import { RouterModule } from '@angular/router';
import { WishlistToggleComponent } from '../../../shared/components/wishlist-toggle/wishlist-toggle';

@Component({
  selector: 'app-wishlist',
  standalone: true,
  imports: [CommonModule, RouterModule, WishlistToggleComponent],
  templateUrl: './wishlist.html'
})
export class WishlistComponent implements OnInit {
  private wishlistService = inject(WishlistService);
  private cartService = inject(CartService);
  private toast = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);
  private platformId = inject(PLATFORM_ID);

  wishlistItems: WishlistItem[] = [];
  isLoading = true;

  ngOnInit(): void {
    if (isPlatformBrowser(this.platformId)) {
      this.loadWishlist();
    }
  }

  loadWishlist(): void {
    this.isLoading = true;
    this.wishlistService.getMyWishlist().subscribe({
      next: (items) => {
        this.wishlistItems = items;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.toast.error('Không thể tải danh sách yêu thích');
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  removeItem(productId: string): void {
    this.wishlistService.toggle(productId).subscribe({
      next: (res) => {
        if (!res.isAdded) {
          this.wishlistItems = this.wishlistItems.filter(item => item.productId !== productId);
          this.toast.success('Đã xóa khỏi danh sách yêu thích');
          this.cdr.detectChanges();
        }
      }
    });
  }

  addToCart(item: WishlistItem): void {
    if (item.stockQuantity <= 0) return;

    const product: ProductCard = {
      id: item.productId,
      name: item.productName,
      slug: item.productSlug,
      price: item.discountPrice || item.price,
      regularPrice: item.price,
      salePrice: item.discountPrice || null,
      image: item.productImage || '',
      category: '',
      brand: '',
      brandId: '',
      stockQuantity: item.stockQuantity,
      warrantyMonths: 0,
      specs: {}
    };

    this.cartService.addToCart(product);
    this.toast.success(`Đã thêm ${item.productName} vào giỏ hàng!`);
  }
}
