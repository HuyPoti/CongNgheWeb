import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { InventoryService } from '../../../../core/services/inventory.service';
import { InventoryReceipt } from '../../../../core/models/inventory.model';

@Component({
  selector: 'app-inventory-receipt-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './inventory-receipt-list.html'
})
export class InventoryReceiptList implements OnInit {
  private inventoryService = inject(InventoryService);
  private router = inject(Router);

  receipts = signal<InventoryReceipt[]>([]);
  isLoading = signal<boolean>(false);

  ngOnInit() {
    this.loadReceipts();
  }

  loadReceipts() {
    this.isLoading.set(true);
    this.inventoryService.getReceipts().subscribe({
      next: (data) => {
        this.receipts.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Lỗi khi tải phiếu nhập kho', err);
        this.isLoading.set(false);
      }
    });
  }

  viewDetail(id: string) {
    this.router.navigate(['/admin/inventory-receipts', id]);
  }
}
