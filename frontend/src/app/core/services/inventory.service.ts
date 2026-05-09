import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { 
  InventoryReceipt, 
  CreateInventoryReceiptDto, 
  InventoryTransaction,
  AdjustStockDto,
  CancelReceiptRequest,
  StockStatus
} from '../models/inventory.model';
import { environment } from '../../../environments/environment';

interface ApiResponse<T> {
  status: string;
  data: T;
  message: string;
  error?: unknown;
}

@Injectable({
  providedIn: 'root'
})
export class InventoryService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/Inventory`;

  getReceipts(): Observable<InventoryReceipt[]> {
    return this.http.get<ApiResponse<InventoryReceipt[]>>(`${this.apiUrl}/receipts`)
      .pipe(map(res => res.data));
  }

  getReceiptById(id: string): Observable<InventoryReceipt> {
    return this.http.get<ApiResponse<InventoryReceipt>>(`${this.apiUrl}/receipts/${id}`)
      .pipe(map(res => res.data));
  }

  createReceipt(dto: CreateInventoryReceiptDto): Observable<InventoryReceipt> {
    return this.http.post<ApiResponse<InventoryReceipt>>(`${this.apiUrl}/receipts`, dto)
      .pipe(map(res => res.data));
  }

  completeReceipt(id: string): Observable<InventoryReceipt> {
    return this.http.patch<ApiResponse<InventoryReceipt>>(`${this.apiUrl}/receipts/${id}/complete`, {})
      .pipe(map(res => res.data));
  }

  cancelReceipt(id: string, reason: string): Observable<InventoryReceipt> {
    const request: CancelReceiptRequest = { reason };
    return this.http.patch<ApiResponse<InventoryReceipt>>(`${this.apiUrl}/receipts/${id}/cancel`, request)
      .pipe(map(res => res.data));
  }

  getTransactions(productId: string): Observable<InventoryTransaction[]> {
    return this.http.get<ApiResponse<InventoryTransaction[]>>(`${this.apiUrl}/transactions/${productId}`)
      .pipe(map(res => res.data));
  }

  adjustStock(dto: AdjustStockDto): Observable<InventoryTransaction> {
    return this.http.post<ApiResponse<InventoryTransaction>>(`${this.apiUrl}/adjust`, dto)
      .pipe(map(res => res.data));
  }

  getStockStatus(): Observable<StockStatus[]> {
    return this.http.get<ApiResponse<StockStatus[]>>(`${this.apiUrl}/stock-status`)
      .pipe(map(res => res.data));
  }
}
