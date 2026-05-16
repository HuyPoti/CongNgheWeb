import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Brand, CreateBrand, UpdateBrand } from '../models/brand.model';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';

@Injectable({ providedIn: 'root' })
export class BrandService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl + '/brands';

  getAll(): Observable<Brand[]> {
    return this.http.get<ApiResponse<Brand[]>>(this.apiUrl).pipe(map((res) => res.data));
  }

  getById(id: string): Observable<Brand> {
    return this.http.get<ApiResponse<Brand>>(`${this.apiUrl}/${id}`).pipe(map((res) => res.data));
  }

  getBySlug(slug: string): Observable<Brand> {
    return this.http.get<ApiResponse<Brand>>(`${this.apiUrl}/slug/${slug}`).pipe(map((res) => res.data));
  }

  create(brand: CreateBrand): Observable<Brand> {
    return this.http.post<ApiResponse<Brand>>(this.apiUrl, brand).pipe(map((res) => res.data));
  }

  update(id: string, brand: UpdateBrand): Observable<Brand> {
    return this.http.put<ApiResponse<Brand>>(`${this.apiUrl}/${id}`, brand).pipe(map((res) => res.data));
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
