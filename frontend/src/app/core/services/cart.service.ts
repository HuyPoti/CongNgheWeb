import { computed, Injectable, signal } from '@angular/core';
import { ProductCard } from '../models/product.model';

/** Mini-interface tránh circular dependency với FlashSaleService */
export interface FlashSaleItemRef {
  productId: string;
  regularPrice: number;
  flashPrice: number;
  isSoldOut: boolean;
}

export interface CartSyncResult {
  changed: boolean;
  messages: string[];
}

export interface CartItem extends ProductCard {
  quantity: number;
}

@Injectable({
  providedIn: 'root',
})
export class CartService {
  private readonly storageKey = 'cart_items_v1';
  private readonly couponStorageKey = 'applied_coupon_v1';
  private items = signal<CartItem[]>(this.restoreCartItems());
  appliedCoupon = signal<string | null>(this.restoreCoupon());

  // Quick purchase (Buy Now) support
  isBuyNowMode = signal<boolean>(false);
  buyNowItem = signal<CartItem | null>(null);

  checkoutItems = computed(() => {
    if (this.isBuyNowMode()) {
      const item = this.buyNowItem();
      return item ? [item] : [];
    }
    return this.items();
  });

  checkoutSubtotal = computed(() => {
    return this.checkoutItems().reduce((acc, item) => acc + item.price * item.quantity, 0);
  });

  setBuyNow(product: ProductCard | null) {
    if (product) {
      this.buyNowItem.set({ ...product, quantity: 1 });
      this.isBuyNowMode.set(true);
    } else {
      this.buyNowItem.set(null);
      this.isBuyNowMode.set(false);
    }
  }

  completeCheckout() {
    if (this.isBuyNowMode()) {
      this.setBuyNow(null);
    } else {
      this.clearCart();
    }
  }

  getCartItems = computed(() => this.items());

  subtotal = computed(() =>
    this.items().reduce((acc, item) => acc + item.price * item.quantity, 0),
  );

  totalItems = computed(() => this.items().reduce((acc, item) => acc + item.quantity, 0));

  setAppliedCoupon(code: string | null) {
    this.appliedCoupon.set(code);
    if (this.canUseStorage()) {
      if (code) {
        localStorage.setItem(this.couponStorageKey, code);
      } else {
        localStorage.removeItem(this.couponStorageKey);
      }
    }
  }

  private restoreCoupon(): string | null {
    if (!this.canUseStorage()) return null;
    return localStorage.getItem(this.couponStorageKey);
  }

  addToCart(product: ProductCard) {
    this.items.update((items) => {
      const existing = items.find((item) => item.id === product.id);
      if (existing) {
        const nextItems = items.map((item) =>
          item.id === product.id ? { ...item, quantity: item.quantity + 1 } : item,
        );
        this.persist(nextItems);
        return nextItems;
      }
      const nextItems = [...items, { ...product, quantity: 1 }];
      this.persist(nextItems);
      return nextItems;
    });
  }

  removeFromCart(productId: string) {
    this.items.update((items) => {
      const nextItems = items.filter((item) => item.id !== productId);
      this.persist(nextItems);
      return nextItems;
    });
  }

  updateQuantity(productId: string, delta: number) {
    this.items.update((items) => {
      const nextItems = items.map((item) =>
        item.id === productId ? { ...item, quantity: Math.max(1, item.quantity + delta) } : item,
      );
      this.persist(nextItems);
      return nextItems;
    });
  }

  clearCart() {
    this.items.set([]);
    this.persist([]);
    this.setAppliedCoupon(null);
  }

  /**
   * Re-validate giá cart items với dữ liệu flash sale hiện tại.
   * Gọi khi user mở CartPage để đảm bảo giá luôn đúng.
   */
  syncFlashSalePrices(flashItems: FlashSaleItemRef[]): CartSyncResult {
    const messages: string[] = [];
    let changed = false;

    this.items.update((items) => {
      const updated = items.map((item) => {
        const flashItem = flashItems.find((fi) => fi.productId === item.id);

        if (flashItem && !flashItem.isSoldOut) {
          // Case 1: item chưa có giá flash → áp dụng flash price
          if (!item.isFlashSale) {
            changed = true;
            messages.push(`🔥 "${item.name}" đang có Flash Sale! Giá đã được cập nhật.`);
            return {
              ...item,
              price: flashItem.flashPrice,
              regularPrice: flashItem.regularPrice,
              isFlashSale: true,
            };
          }
          // Case 2: giá flash thay đổi (admin cập nhật giữa chừng)
          if (item.price !== flashItem.flashPrice) {
            changed = true;
            messages.push(`ℹ️ Giá Flash Sale của "${item.name}" vừa được cập nhật.`);
            return {
              ...item,
              price: flashItem.flashPrice,
              regularPrice: flashItem.regularPrice,
            };
          }
        } else if (item.isFlashSale) {
          // Case 3: flash sale đã kết thúc → hoàn về giá gốc
          changed = true;
          messages.push(`⏰ Flash Sale của "${item.name}" đã kết thúc. Giá đã được cập nhật về giá gốc.`);
          const revertPrice = item.salePrice ?? item.regularPrice;
          return {
            ...item,
            price: revertPrice,
            isFlashSale: false,
          };
        }

        return item;
      });

      if (changed) this.persist(updated);
      return updated;
    });

    return { changed, messages };
  }

  private persist(items: CartItem[]) {
    if (!this.canUseStorage()) return;
    localStorage.setItem(this.storageKey, JSON.stringify(items));
  }

  private restoreCartItems(): CartItem[] {
    if (!this.canUseStorage()) return [];

    const raw = localStorage.getItem(this.storageKey);
    if (!raw) return [];

    try {
      const parsed = JSON.parse(raw) as CartItem[];
      return Array.isArray(parsed) ? parsed : [];
    } catch {
      localStorage.removeItem(this.storageKey);
      return [];
    }
  }

  private canUseStorage(): boolean {
    return typeof window !== 'undefined' && typeof localStorage !== 'undefined';
  }
}
