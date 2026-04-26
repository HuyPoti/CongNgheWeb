import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-wishlist',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './wishlist.html'
})
export class WishlistComponent {
  wishlistItems = [
    { id: 'p01', name: 'Chuột ASUS ROG Harpe Ace Aim Lab Edition', price: 3590000, oldPrice: 3990000, image: 'https://dlcdnwebimgs.asus.com/gain/392DE691-8AF2-4FD0-8914-996489370894', inStock: true },
    { id: 'p02', name: 'Bàn phím ASUS ROG Azoth Night City', price: 5290000, oldPrice: 5990000, image: 'https://dlcdnwebimgs.asus.com/gain/392DE691-8AF2-4FD0-8914-996489370894', inStock: true },
    { id: 'p03', name: 'Tai nghe ASUS ROG Delta S Animate', price: 4590000, oldPrice: 4990000, image: 'https://dlcdnwebimgs.asus.com/gain/392DE691-8AF2-4FD0-8914-996489370894', inStock: false }
  ];

  removeItem(id: string) {
    this.wishlistItems = this.wishlistItems.filter(item => item.id !== id);
  }

  addToCart(item: any) {
    if (!item.inStock) return;
    alert(`Đã thêm ${item.name} vào giỏ hàng!`);
  }
}
