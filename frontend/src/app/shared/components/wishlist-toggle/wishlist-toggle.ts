import { Component, Input, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WishlistService } from '../../../core/services/wishlist.service';
import { ToastService } from '../../../core/services/toast.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-wishlist-toggle',
  standalone: true,
  imports: [CommonModule],
  template: `
    <button (click)="toggle($event)" 
            [class.active]="isInWishlist"
            class="wishlist-btn transition-all duration-300"
            [class.is-active]="isInWishlist"
            [title]="isInWishlist ? 'Xóa khỏi yêu thích' : 'Thêm vào yêu thích'">
      <span class="material-symbols-outlined text-[20px] transition-all duration-500 group-hover:scale-125"
            [class.filled]="isInWishlist">
        {{ isInWishlist ? 'favorite' : 'favorite' }}
      </span>
    </button>
  `,
  styles: [`
    .wishlist-btn {
      width: 40px;
      height: 40px;
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 12px;
      background: rgba(255, 255, 255, 0.9);
      backdrop-filter: blur(8px);
      color: #94a3b8;
      border: 1px solid #f1f5f9;
      opacity: 1;
      transform: translateY(0);
    }
    
    /* Hiệu ứng mờ khi chưa thích và chưa hover trên card (sẽ được handle bởi CSS bên ngoài nếu cần, hoặc mặc định hiện nhẹ) */
    :host-context(.group):host:not(:hover) .wishlist-btn:not(.is-active) {
      opacity: 0.4;
    }
    
    :host-context(.group):hover .wishlist-btn {
      opacity: 1;
      transform: translateY(0);
    }

    /* Luôn hiện nếu đã thích */
    .wishlist-btn.is-active {
      opacity: 1 !important;
      transform: translateY(0) !important;
      color: #ef4444;
      border-color: #fee2e2;
      background: #fff1f2;
    }
    .wishlist-btn:hover {
      background: white;
      color: #ef4444;
      box-shadow: 0 4px 15px rgba(239, 68, 68, 0.2);
    }
    .filled {
      font-variation-settings: 'FILL' 1, 'wght' 400, 'GRAD' 0, 'opsz' 24;
      color: #ef4444 !important;
    }
  `]
})
export class WishlistToggleComponent implements OnInit {
  @Input({ required: true }) productId!: string;
  @Input() isLiked: boolean | null = null;
  
  private wishlistService = inject(WishlistService);
  private authService = inject(AuthService);
  private toast = inject(ToastService);

  isInWishlist = false;

  ngOnInit(): void {
    if (this.isLiked !== null) {
      this.isInWishlist = this.isLiked;
      return;
    }

    if (this.authService.isLoggedIn()) {
      this.checkStatus();
    }
  }

  checkStatus(): void {
    this.wishlistService.check(this.productId).subscribe({
      next: (res) => this.isInWishlist = res.isInWishlist
    });
  }

  toggle(event: Event): void {
    event.stopPropagation();
    event.preventDefault();

    if (!this.authService.isLoggedIn()) {
      this.toast.info('Vui lòng đăng nhập để lưu sản phẩm yêu thích');
      return;
    }

    this.wishlistService.toggle(this.productId).subscribe({
      next: (res) => {
        this.isInWishlist = res.isAdded;
        if (this.isInWishlist) {
          this.toast.success('Đã thêm vào danh sách yêu thích');
        }
      },
      error: (err) => {
        if (err.status === 400) {
          this.toast.warning('Danh sách yêu thích đã đầy (tối đa 50 sản phẩm)');
        }
      }
    });
  }
}
