import { Injectable, signal, computed } from '@angular/core';
import { ProductCard } from '../models/product.model';

export interface CartItem extends ProductCard {
  quantity: number;
}

@Injectable({
  providedIn: 'root',
})
export class CartService {
  private items = signal<CartItem[]>([]);

  getCartItems = computed(() => this.items());

  subtotal = computed(() =>
    this.items().reduce((acc, item) => acc + item.price * item.quantity, 0),
  );

  totalItems = computed(() => this.items().reduce((acc, item) => acc + item.quantity, 0));

  addToCart(product: ProductCard) {
    this.items.update((items) => {
      const existing = items.find((item) => item.id === product.id);
      if (existing) {
        return items.map((item) =>
          item.id === product.id ? { ...item, quantity: item.quantity + 1 } : item,
        );
      }
      return [...items, { ...product, quantity: 1 }];
    });
  }

  removeFromCart(productId: string) {
    this.items.update((items) => items.filter((item) => item.id !== productId));
  }

  updateQuantity(productId: string, delta: number) {
    this.items.update((items) =>
      items.map((item) =>
        item.id === productId ? { ...item, quantity: Math.max(1, item.quantity + delta) } : item,
      ),
    );
  }

  clearCart() {
    this.items.set([]);
  }
}
