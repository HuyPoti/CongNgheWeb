import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Supplier, CreateSupplierDto, UpdateSupplierDto } from '../models/supplier.model';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';

@Injectable({
  providedIn: 'root'
})
export class SupplierService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Suppliers`;

  getSuppliers(): Observable<Supplier[]> {
    return this.http.get<ApiResponse<Supplier[]>>(this.apiUrl)
      .pipe(map(res => res.data));
  }

  getSupplierById(id: string): Observable<Supplier> {
    return this.http.get<ApiResponse<Supplier>>(`${this.apiUrl}/${id}`)
      .pipe(map(res => res.data));
  }

  createSupplier(dto: CreateSupplierDto): Observable<Supplier> {
    return this.http.post<ApiResponse<Supplier>>(this.apiUrl, dto)
      .pipe(map(res => res.data));
  }

  updateSupplier(id: string, dto: UpdateSupplierDto): Observable<object> {
    return this.http.put<ApiResponse<object>>(`${this.apiUrl}/${id}`, dto)
      .pipe(map(res => res.data));
  }

  deleteSupplier(id: string): Observable<object> {
    return this.http.delete<ApiResponse<object>>(`${this.apiUrl}/${id}`)
      .pipe(map(res => res.data));
  }
}
