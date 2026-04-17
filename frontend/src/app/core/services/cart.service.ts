import { computed, Injectable, signal } from '@angular/core';
import { ProductCard } from '../models/product.model';

export interface CartItem extends ProductCard {
  quantity: number;
}

@Injectable({
  providedIn: 'root',
})
export class CartService {
  private readonly storageKey = 'cart_items_v1';
  private items = signal<CartItem[]>(this.restoreCartItems());

  getCartItems = computed(() => this.items());

  subtotal = computed(() =>
    this.items().reduce((acc, item) => acc + item.price * item.quantity, 0),
  );

  totalItems = computed(() => this.items().reduce((acc, item) => acc + item.quantity, 0));

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
