import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Brand, CreateBrand, UpdateBrand } from '../models/brand.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class BrandService {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl + '/brands';
  getAll(): Observable<Brand[]> {
    return this.http.get<Brand[]>(this.apiUrl);
  }
  getById(id: string): Observable<Brand> {
    return this.http.get<Brand>(`${this.apiUrl}/${id}`);
  }
  getBySlug(slug: string): Observable<Brand> {
    return this.http.get<Brand>(`${this.apiUrl}/slug/${slug}`);
  }
  create(brand: CreateBrand): Observable<Brand> {
    return this.http.post<Brand>(this.apiUrl, brand);
  }
  update(id: string, brand: UpdateBrand): Observable<Brand> {
    return this.http.put<Brand>(`${this.apiUrl}/${id}`, brand);
  }
  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
