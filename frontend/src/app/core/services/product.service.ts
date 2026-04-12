import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
  ProductListResponse,
  ProductDto,
  ProductFullDto,
  PagedProductResponse,
  CreateProductDto,
  UpdateProductDto,
  ProductImageDto,
  ProductSpecDto,
  CreateProductImageDto,
  CreateProductSpecDto,
} from '../models/product.model';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/product`;
  private productsUrl = `${environment.apiUrl}/products`;
  private clientProductsCache = new Map<string, ProductListResponse>();

  clearCache(): void {
    this.clientProductsCache.clear();
  }

  // ── Client-facing ──────────────────────────────────────────────

  fetchClientProducts(page = 1, pageSize = 12): Observable<ProductListResponse> {
    const cacheKey = `${page}-${pageSize}`;
    if (this.clientProductsCache.has(cacheKey)) {
      return of(this.clientProductsCache.get(cacheKey)!);
    }
    return this.http.get<ProductListResponse>(
      `${this.baseUrl}/client?page=${page}&pageSize=${pageSize}`,
    ).pipe(
      tap((res) => {
        this.clientProductsCache.set(cacheKey, res);
      })
    );
  }

  // ── Admin CRUD ─────────────────────────────────────────────────

  getAll(opts?: {
    keyword?: string;
    categoryId?: string;
    minPrice?: number;
    maxPrice?: number;
    page?: number;
    pageSize?: number;
  }): Observable<PagedProductResponse> {
    let params = new HttpParams()
      .set('page', String(opts?.page ?? 1))
      .set('pageSize', String(opts?.pageSize ?? 20));
    if (opts?.keyword) params = params.set('keyword', opts.keyword);
    if (opts?.categoryId) params = params.set('categoryId', opts.categoryId);
    if (opts?.minPrice != null) params = params.set('minPrice', String(opts.minPrice));
    if (opts?.maxPrice != null) params = params.set('maxPrice', String(opts.maxPrice));
    return this.http.get<PagedProductResponse>(this.baseUrl, { params });
  }

  getById(id: string): Observable<ProductDto> {
    return this.http.get<ProductDto>(`${this.baseUrl}/${id}`);
  }

  getFullById(id: string): Observable<ProductFullDto> {
    return this.http.get<ProductFullDto>(`${this.baseUrl}/${id}/full`);
  }

  getBySlug(slug: string): Observable<ProductDto> {
    return this.http.get<ProductDto>(`${this.baseUrl}/slug/${slug}`);
  }

  create(dto: CreateProductDto): Observable<ProductDto> {
    return this.http.post<ProductDto>(this.baseUrl, dto).pipe(
      tap(() => this.clearCache())
    );
  }

  update(id: string, dto: UpdateProductDto): Observable<ProductDto> {
    return this.http.put<ProductDto>(`${this.baseUrl}/${id}`, dto).pipe(
      tap(() => this.clearCache())
    );
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`).pipe(
      tap(() => this.clearCache())
    );
  }

  // ── Product Images ─────────────────────────────────────────────

  getImages(productId: string): Observable<ProductImageDto[]> {
    return this.http.get<ProductImageDto[]>(`${this.productsUrl}/${productId}/images`);
  }

  addImage(productId: string, dto: CreateProductImageDto): Observable<ProductImageDto> {
    return this.http.post<ProductImageDto>(`${this.productsUrl}/${productId}/images`, dto);
  }

  deleteImage(productId: string, imageId: string): Observable<void> {
    return this.http.delete<void>(`${this.productsUrl}/${productId}/images/${imageId}`);
  }

  // ── Product Specs ──────────────────────────────────────────────

  getSpecs(productId: string): Observable<ProductSpecDto[]> {
    return this.http.get<ProductSpecDto[]>(`${this.productsUrl}/${productId}/specs`);
  }

  addSpec(productId: string, dto: CreateProductSpecDto): Observable<ProductSpecDto> {
    return this.http.post<ProductSpecDto>(`${this.productsUrl}/${productId}/specs`, dto);
  }

  updateSpec(
    productId: string,
    specId: string,
    dto: CreateProductSpecDto,
  ): Observable<ProductSpecDto> {
    return this.http.put<ProductSpecDto>(`${this.productsUrl}/${productId}/specs/${specId}`, dto);
  }

  deleteSpec(productId: string, specId: string): Observable<void> {
    return this.http.delete<void>(`${this.productsUrl}/${productId}/specs/${specId}`);
  }
}
