import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';

export interface WishlistItem {
  wishlistId: string;
  productId: string;
  productName: string;
  productSlug: string;
  productImage: string;
  price: number;
  discountPrice?: number;
  stockQuantity: number;
  createdAt: string;
}

@Injectable({
  providedIn: 'root'
})
export class WishlistService {
  private apiUrl = `${environment.apiUrl}/wishlist`;

  constructor(private http: HttpClient) { }

  getMyWishlist(): Observable<WishlistItem[]> {
    return this.http.get<WishlistItem[]>(this.apiUrl);
  }

  toggle(productId: string): Observable<{ isAdded: boolean; message: string }> {
    return this.http.post<{ isAdded: boolean; message: string }>(`${this.apiUrl}/toggle/${productId}`, {});
  }

  check(productId: string): Observable<{ isInWishlist: boolean }> {
    return this.http.get<{ isInWishlist: boolean }>(`${this.apiUrl}/check/${productId}`);
  }
}
