import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ClientProductsQuery,
  ProductListResponse,
  ProductDto,
  ProductFullDto,
  PagedProductResponse,
  CreateProductDto,
  UpdateProductDto,
  ProductImageDto,
  CreateProductImageDto,
} from '../models/product.model';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/product`;
  private productsUrl = `${environment.apiUrl}/products`;

  fetchClientProducts(query: ClientProductsQuery = {}): Observable<ProductListResponse> {
    let params = new HttpParams()
      .set('page', String(query.page ?? 1))
      .set('pageSize', String(query.pageSize ?? 12));

    if (query.categorySlug) params = params.set('categorySlug', query.categorySlug);
    if (query.keyword?.trim()) params = params.set('keyword', query.keyword.trim());
    if (query.brandId) params = params.set('brandId', query.brandId);
    if (query.minPrice != null) params = params.set('minPrice', String(query.minPrice));
    if (query.maxPrice != null) params = params.set('maxPrice', String(query.maxPrice));
    if (query.sortBy) params = params.set('sortBy', query.sortBy);

    return this.http.get<ProductListResponse>(`${this.baseUrl}/client`, { params });
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

  getFullBySlug(slug: string): Observable<ProductFullDto> {
    return this.http.get<ProductFullDto>(`${this.baseUrl}/slug/${slug}/full`);
  }

  create(dto: CreateProductDto): Observable<ProductDto> {
    return this.http.post<ProductDto>(this.baseUrl, dto);
  }

  update(id: string, dto: UpdateProductDto): Observable<ProductDto> {
    return this.http.put<ProductDto>(`${this.baseUrl}/${id}`, dto);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
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
}
