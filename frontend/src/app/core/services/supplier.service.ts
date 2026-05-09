import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { Supplier, CreateSupplierDto, UpdateSupplierDto } from '../models/supplier.model';
import { environment } from '../../../environments/environment';

interface ApiResponse<T> {
  status: string;
  data: T;
  message: string;
  error?: any;
}

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

  updateSupplier(id: string, dto: UpdateSupplierDto): Observable<any> {
    return this.http.put<ApiResponse<any>>(`${this.apiUrl}/${id}`, dto)
      .pipe(map(res => res.data));
  }

  deleteSupplier(id: string): Observable<any> {
    return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/${id}`)
      .pipe(map(res => res.data));
  }
}
