import { ProductDto } from './product.model';
import { Supplier } from './supplier.model';
import { User } from './user.model'; // Assumes user model exists, otherwise use any

export interface InventoryReceipt {
  receiptId: string;
  receiptCode: string;
  supplierId: string;
  totalAmount: number;
  status: number; // 1: Draft, 2: Completed, 3: Cancelled
  notes: string | null;
  createdBy: string;
  createdAt: string;
  updatedAt: string;
  supplier?: Supplier;
  creator?: User;
  items: InventoryReceiptItem[];
}

export interface InventoryReceiptItem {
  itemId: string;
  receiptId: string;
  productId: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
  product?: ProductDto;
}

export interface InventoryTransaction {
  transactionId: string;
  productId: string;
  transactionType: number; // 1: Nhập kho, 2: Xuất bán, 3: Hoàn trả, 4: Xuất hủy/Điều chỉnh, 5: Khác
  referenceId: string | null;
  quantityChanged: number;
  stockAfter: number;
  notes: string | null;
  createdBy: string | null;
  createdAt: string;
  product?: ProductDto;
  creator?: User;
}

export interface CreateInventoryReceiptItemDto {
  productId: string;
  quantity: number;
  unitPrice: number;
}

export interface CreateInventoryReceiptDto {
  supplierId: string;
  notes: string;
  items: CreateInventoryReceiptItemDto[];
}

export interface AdjustStockDto {
  productId: string;
  quantityChanged: number;
  transactionType: number;
  notes: string;
}

export interface CancelReceiptRequest {
  reason: string;
}

export interface StockStatus {
  productId: string;
  productName: string;
  productSku: string | null;
  currentStock: number;
  lastUpdatedAt: string;
}
