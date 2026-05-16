import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiResponse } from '../models/api-response.model';

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
  providedIn: 'root',
})
export class WishlistService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/wishlist`;

  getMyWishlist(): Observable<WishlistItem[]> {
    return this.http.get<ApiResponse<WishlistItem[]>>(this.apiUrl).pipe(map((res) => res.data));
  }

  toggle(productId: string): Observable<{ isAdded: boolean; message: string }> {
    return this.http
      .post<ApiResponse<{ isAdded: boolean; message: string }>>(
        `${this.apiUrl}/toggle/${productId}`,
        {}
      )
      .pipe(map((res) => res.data));
  }

  check(productId: string): Observable<{ isInWishlist: boolean }> {
    return this.http
      .get<ApiResponse<{ isInWishlist: boolean }>>(`${this.apiUrl}/check/${productId}`)
      .pipe(map((res) => res.data));
  }
}
