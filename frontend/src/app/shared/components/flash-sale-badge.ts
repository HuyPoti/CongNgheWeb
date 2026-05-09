import { Component, Input, OnInit, OnDestroy, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface FlashSaleInfo {
  flashPrice: number;
  regularPrice: number;
  endTime: string; // ISO datetime
  isSoldOut: boolean;
}

@Component({
  selector: 'app-flash-sale-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="flash-badge" *ngIf="flashPrice > 0 && regularPrice > 0">
      @if (isSoldOut) {
        <div class="sold-out-overlay">
          <span class="sold-out-text">Hết Hàng</span>
        </div>
      }

      <div class="badge-content">
        <div class="flash-label">
          <span class="lightning">⚡</span>
          <span>FLASH SALE</span>
        </div>

        <div class="price-section">
          <div class="flash-price">
            {{ flashPrice | number:'1.0-0' }} ₫
          </div>
          <div class="original-price">
            {{ regularPrice | number:'1.0-0' }} ₫
          </div>
          <div class="discount-percent">
            -{{ discountPercent() }}%
          </div>
        </div>

        <div class="countdown" *ngIf="!isExpired() && !isSoldOut">
          <span class="countdown-icon">⏱</span>
          <span class="countdown-text">{{ countdownDisplay() }}</span>
        </div>

        <div class="expired-message" *ngIf="isExpired()">
          <span>Đã kết thúc</span>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .flash-badge {
      position: relative;
      display: inline-block;
      background: linear-gradient(135deg, #ff6b35 0%, #f7931e 100%);
      border-radius: 8px;
      padding: 8px;
      min-width: 100px;
      font-size: 12px;
      color: white;
      font-weight: 600;
      box-shadow: 0 4px 12px rgba(255, 107, 53, 0.3);
      border: 2px solid #ff4500;
    }

    .sold-out-overlay {
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.7);
      border-radius: 6px;
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 10;
    }

    .sold-out-text {
      font-size: 14px;
      font-weight: 700;
      color: white;
      text-shadow: 0 2px 4px rgba(0, 0, 0, 0.5);
    }

    .badge-content {
      display: flex;
      flex-direction: column;
      gap: 6px;
    }

    .flash-label {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 11px;
      letter-spacing: 0.5px;
      text-transform: uppercase;
    }

    .lightning {
      font-size: 13px;
      animation: pulse 1s ease-in-out infinite;
    }

    @keyframes pulse {
      0%, 100% { opacity: 1; }
      50% { opacity: 0.5; }
    }

    .price-section {
      display: flex;
      align-items: baseline;
      gap: 6px;
      margin: 4px 0;
    }

    .flash-price {
      font-size: 14px;
      font-weight: 700;
      color: #fff;
    }

    .original-price {
      font-size: 11px;
      color: rgba(255, 255, 255, 0.8);
      text-decoration: line-through;
    }

    .discount-percent {
      font-size: 11px;
      background: rgba(0, 0, 0, 0.3);
      padding: 2px 6px;
      border-radius: 3px;
      margin-left: auto;
    }

    .countdown {
      display: flex;
      align-items: center;
      gap: 4px;
      font-size: 11px;
      padding: 4px 6px;
      background: rgba(0, 0, 0, 0.2);
      border-radius: 4px;
      text-align: center;
    }

    .countdown-icon {
      font-size: 12px;
    }

    .countdown-text {
      font-weight: 600;
      letter-spacing: 0.5px;
    }

    .expired-message {
      font-size: 11px;
      padding: 4px 6px;
      background: rgba(0, 0, 0, 0.2);
      border-radius: 4px;
      text-align: center;
      opacity: 0.8;
    }
  `]
})
export class FlashSaleBadgeComponent implements OnInit, OnDestroy {
  @Input() flashPrice = 0;
  @Input() regularPrice = 0;
  @Input() endTime = '';
  @Input() isSoldOut = false;

  discountPercent = signal(0);
  countdownDisplay = signal('');
  isExpired = signal(false);
  private countdownInterval: any;

  ngOnInit() {
    this.calculateDiscount();
    this.updateCountdown();

    // Update countdown every second
    this.countdownInterval = setInterval(() => {
      this.updateCountdown();
    }, 1000);
  }

  ngOnDestroy() {
    if (this.countdownInterval) {
      clearInterval(this.countdownInterval);
    }
  }

  private calculateDiscount() {
    if (this.regularPrice > 0) {
      const percent = Math.round(((this.regularPrice - this.flashPrice) / this.regularPrice) * 100);
      this.discountPercent.set(Math.max(0, percent));
    }
  }

  private updateCountdown() {
    const now = new Date().getTime();
    const endDate = new Date(this.endTime).getTime();
    const diff = endDate - now;

    if (diff <= 0) {
      this.isExpired.set(true);
      this.countdownDisplay.set('Kết thúc');
      return;
    }

    this.isExpired.set(false);

    const days = Math.floor(diff / (1000 * 60 * 60 * 24));
    const hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
    const seconds = Math.floor((diff % (1000 * 60)) / 1000);

    if (days > 0) {
      this.countdownDisplay.set(`${days}d ${hours}h`);
    } else if (hours > 0) {
      this.countdownDisplay.set(`${hours}h ${minutes}m`);
    } else {
      this.countdownDisplay.set(`${minutes}m ${seconds}s`);
    }
  }
}
