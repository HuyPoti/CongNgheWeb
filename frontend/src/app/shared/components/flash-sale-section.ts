import { Component, OnInit, OnDestroy, inject, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FlashSaleService, FlashSaleDto, FlashSaleItemDto } from '../../core/services/flash-sale.service';
import { FlashSaleBadgeComponent } from './flash-sale-badge';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-flash-sale-section',
  standalone: true,
  imports: [CommonModule, FlashSaleBadgeComponent],
  template: `
    <div class="flash-sale-section" *ngIf="flashSale() && isWithinTimeframe()">
      <div class="section-container">
        <!-- Header -->
        <div class="section-header">
          <div class="header-left">
            <div class="section-title-group">
              <span class="lightning-icon">⚡</span>
              <h2 class="section-title">{{ flashSale()!.title }}</h2>
            </div>
            <p class="section-subtitle">Giảm giá sốc - Hạn có - Số lượng có hạn</p>
          </div>

          <div class="header-right">
            <div class="countdown-box">
              <span class="countdown-label">Kết thúc trong:</span>
              <span class="countdown-value">{{ countdownDisplay() }}</span>
            </div>
          </div>
        </div>

        <!-- Products Grid -->
        <div class="products-grid">
          @if (flashSale()!.items.length === 0) {
            <div class="empty-state">
              <p>Không có sản phẩm flash sale</p>
            </div>
          } @else {
            @for (item of flashSale()!.items; track item.id) {
              <div class="product-card" [class.sold-out]="item.isSoldOut">
                @if (item.isSoldOut) {
                  <div class="sold-out-badge">Hết Hàng</div>
                }

                <div class="product-image-placeholder">
                  <span>📦 Sản phẩm {{ item.productId }}</span>
                </div>

                <div class="product-info">
                  <p class="product-name">{{ item.productName }}</p>

                  <div class="price-row">
                    <span class="flash-price">{{ item.flashPrice | number:'1.0-0' }} ₫</span>
                  </div>

                  <div class="stock-progress">
                    <div class="progress-bar">
                      <div class="progress-fill" 
                        [style.width.%]="getProgressPercent(item)">
                      </div>
                    </div>
                    <span class="stock-label">
                      Đã bán: {{ item.soldCount }}/{{ item.stockLimit }}
                    </span>
                  </div>

                  <button 
                    class="btn-add-cart" 
                    [disabled]="item.isSoldOut"
                    (click)="onAddToCart(item)"
                  >
                    {{ item.isSoldOut ? 'Hết Hàng' : 'Thêm Vào Giỏ' }}
                  </button>
                </div>
              </div>
            }
          }
        </div>
      </div>
    </div>
  `,
  styles: [`
    .flash-sale-section {
      background: linear-gradient(135deg, rgba(255, 107, 53, 0.05) 0%, rgba(247, 147, 30, 0.05) 100%);
      border: 2px solid rgba(255, 107, 53, 0.2);
      border-radius: 12px;
      padding: 24px;
      margin: 24px 0;
    }

    .section-container {
      max-width: 1200px;
      margin: 0 auto;
    }

    .section-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 24px;
      margin-bottom: 24px;
      flex-wrap: wrap;
    }

    .header-left {
      flex: 1;
      min-width: 200px;
    }

    .section-title-group {
      display: flex;
      align-items: center;
      gap: 12px;
      margin-bottom: 8px;
    }

    .lightning-icon {
      font-size: 32px;
      animation: pulse 1s ease-in-out infinite;
    }

    @keyframes pulse {
      0%, 100% { opacity: 1; transform: scale(1); }
      50% { opacity: 0.7; transform: scale(1.1); }
    }

    .section-title {
      margin: 0;
      font-size: 28px;
      font-weight: 700;
      color: #ff6b35;
      letter-spacing: -0.5px;
    }

    .section-subtitle {
      margin: 0;
      font-size: 14px;
      color: #666;
      font-weight: 500;
    }

    .header-right {
      display: flex;
      align-items: center;
      gap: 16px;
    }

    .countdown-box {
      background: linear-gradient(135deg, #ff6b35 0%, #f7931e 100%);
      color: white;
      padding: 12px 20px;
      border-radius: 8px;
      text-align: center;
      box-shadow: 0 4px 12px rgba(255, 107, 53, 0.3);
    }

    .countdown-label {
      display: block;
      font-size: 12px;
      opacity: 0.9;
      margin-bottom: 4px;
    }

    .countdown-value {
      display: block;
      font-size: 16px;
      font-weight: 700;
      letter-spacing: 0.5px;
    }

    .products-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
      gap: 16px;
    }

    .empty-state {
      grid-column: 1 / -1;
      padding: 40px 20px;
      text-align: center;
      color: #999;
    }

    .product-card {
      background: white;
      border: 1px solid #e0e0e0;
      border-radius: 8px;
      overflow: hidden;
      transition: all 0.3s;
      display: flex;
      flex-direction: column;
    }

    .product-card:hover:not(.sold-out) {
      box-shadow: 0 8px 16px rgba(0, 0, 0, 0.1);
      transform: translateY(-4px);
      border-color: #ff6b35;
    }

    .product-card.sold-out {
      opacity: 0.6;
      background: #f5f5f5;
    }

    .sold-out-badge {
      position: absolute;
      top: 8px;
      right: 8px;
      background: #dc3545;
      color: white;
      padding: 6px 12px;
      border-radius: 4px;
      font-size: 12px;
      font-weight: 600;
      z-index: 10;
    }

    .product-image-placeholder {
      position: relative;
      width: 100%;
      aspect-ratio: 4 / 3;
      background: linear-gradient(135deg, #f5f5f5 0%, #e0e0e0 100%);
      display: flex;
      align-items: center;
      justify-content: center;
      color: #999;
      font-size: 13px;
      text-align: center;
      padding: 16px;
    }

    .product-info {
      padding: 12px;
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .product-name {
      margin: 0;
      font-size: 14px;
      font-weight: 600;
      color: #333;
      overflow: hidden;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    .price-row {
      display: flex;
      align-items: baseline;
      gap: 8px;
    }

    .flash-price {
      font-size: 16px;
      font-weight: 700;
      color: #ff6b35;
    }

    .stock-progress {
      display: flex;
      flex-direction: column;
      gap: 4px;
      margin: 4px 0;
    }

    .progress-bar {
      width: 100%;
      height: 6px;
      background: #e0e0e0;
      border-radius: 3px;
      overflow: hidden;
    }

    .progress-fill {
      height: 100%;
      background: linear-gradient(90deg, #ff6b35, #f7931e);
      transition: width 0.3s;
    }

    .stock-label {
      font-size: 11px;
      color: #666;
    }

    .btn-add-cart {
      padding: 8px 12px;
      background: #007bff;
      color: white;
      border: none;
      border-radius: 4px;
      font-weight: 600;
      cursor: pointer;
      font-size: 13px;
      transition: all 0.3s;
      margin-top: auto;
    }

    .btn-add-cart:hover:not(:disabled) {
      background: #0056b3;
      transform: scale(1.02);
    }

    .btn-add-cart:disabled {
      background: #ccc;
      cursor: not-allowed;
    }

    @media (max-width: 768px) {
      .section-header {
        flex-direction: column;
        align-items: flex-start;
      }

      .section-title {
        font-size: 22px;
      }

      .products-grid {
        grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
      }
    }
  `]
})
export class FlashSaleSectionComponent implements OnInit, OnDestroy {
  private flashSaleService = inject(FlashSaleService);
  
  flashSale = signal<FlashSaleDto | null>(null);
  loading = signal(false);
  countdownDisplay = signal('');
  private destroy$ = new Subject<void>();
  private countdownInterval: any;

  ngOnInit() {
    this.loadFlashSale();

    // Reload flash sale every 5 minutes to refresh stock counts
    setInterval(() => {
      this.loadFlashSale();
    }, 5 * 60 * 1000);
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
    if (this.countdownInterval) {
      clearInterval(this.countdownInterval);
    }
  }

  private loadFlashSale() {
    this.loading.set(true);
    this.flashSaleService.getActive()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (sale) => {
          if (sale) {
            this.flashSale.set(sale);
            this.startCountdownUpdate();
          } else {
            this.flashSale.set(null);
          }
          this.loading.set(false);
        },
        error: () => {
          this.loading.set(false);
        }
      });
  }

  private startCountdownUpdate() {
    if (this.countdownInterval) {
      clearInterval(this.countdownInterval);
    }

    this.updateCountdown();
    this.countdownInterval = setInterval(() => {
      this.updateCountdown();
    }, 1000);
  }

  private updateCountdown() {
    const sale = this.flashSale();
    if (!sale) return;

    const now = new Date().getTime();
    const endDate = new Date(sale.endTime).getTime();
    const diff = endDate - now;

    if (diff <= 0) {
      this.flashSale.set(null);
      if (this.countdownInterval) {
        clearInterval(this.countdownInterval);
      }
      return;
    }

    const days = Math.floor(diff / (1000 * 60 * 60 * 24));
    const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
    const seconds = Math.floor((diff % (1000 * 60)) / 1000);

    if (days > 0) {
      this.countdownDisplay.set(`${days}d ${hours}h ${minutes}m`);
    } else if (hours > 0) {
      this.countdownDisplay.set(`${hours}h ${minutes}m ${seconds}s`);
    } else {
      this.countdownDisplay.set(`${minutes}m ${seconds}s`);
    }
  }

  isWithinTimeframe(): boolean {
    const sale = this.flashSale();
    if (!sale) return false;

    const now = new Date();
    const startTime = new Date(sale.startTime);
    const endTime = new Date(sale.endTime);

    return now >= startTime && now <= endTime;
  }

  getProgressPercent(item: FlashSaleItemDto): number {
    if (!item.stockLimit) return 0;
    return Math.min((item.soldCount / item.stockLimit) * 100, 100);
  }

  onAddToCart(item: FlashSaleItemDto) {
    if (item.isSoldOut) return;
    
    // Emit event or navigate to product detail
    // For now, just log it - parent component can handle cart logic
    console.log('Add to cart:', item.productId, item.flashPrice);
    
    // Dispatch custom event for parent to handle
    window.dispatchEvent(new CustomEvent('addToCart', {
      detail: { productId: item.productId, price: item.flashPrice }
    }));
  }
}
