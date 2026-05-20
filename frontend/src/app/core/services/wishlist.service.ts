import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Observable, BehaviorSubject } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { ApiResponse } from '../models/api-response.model';
import { AuthService } from './auth.service';

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
  private authService = inject(AuthService);
  private apiUrl = `${environment.apiUrl}/wishlist`;

  private wishlistIdsSubject = new BehaviorSubject<Set<string>>(new Set<string>());
  public wishlistIds$ = this.wishlistIdsSubject.asObservable();

  constructor() {
    this.authService.currentUser$.subscribe(user => {
      if (user) {
        this.loadWishlistIds();
      } else {
        this.wishlistIdsSubject.next(new Set<string>());
      }
    });
  }

  loadWishlistIds(): void {
    if (!this.authService.isLoggedIn()) return;
    this.http.get<ApiResponse<string[]>>(`${this.apiUrl}/my-ids`).subscribe({
      next: (res) => {
        this.wishlistIdsSubject.next(new Set(res.data));
      }
    });
  }

  getMyWishlist(): Observable<WishlistItem[]> {
    return this.http.get<ApiResponse<WishlistItem[]>>(this.apiUrl).pipe(map((res) => res.data));
  }

  toggle(productId: string): Observable<{ isAdded: boolean; message: string }> {
    return this.http
      .post<ApiResponse<{ isAdded: boolean; message: string }>>(
        `${this.apiUrl}/toggle/${productId}`,
        {}
      )
      .pipe(
        map((res) => res.data),
        tap(data => {
            const currentSet = new Set(this.wishlistIdsSubject.value);
            if (data.isAdded) {
                currentSet.add(productId);
            } else {
                currentSet.delete(productId);
            }
            this.wishlistIdsSubject.next(currentSet);
        })
      );
  }

  check(productId: string): Observable<{ isInWishlist: boolean }> {
    return this.wishlistIds$.pipe(map(set => ({ isInWishlist: set.has(productId) })));
  }
}
