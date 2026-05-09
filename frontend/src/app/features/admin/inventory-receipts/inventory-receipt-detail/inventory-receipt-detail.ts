import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { InventoryService } from '../../../../core/services/inventory.service';
import { InventoryReceipt } from '../../../../core/models/inventory.model';

@Component({
  selector: 'app-inventory-receipt-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './inventory-receipt-detail.html'
})
export class InventoryReceiptDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private inventoryService = inject(InventoryService);

  receipt = signal<InventoryReceipt | null>(null);
  isLoading = signal<boolean>(true);
  isProcessing = signal<boolean>(false);
  cancelReason = signal<string>('');
  showCancelModal = signal<boolean>(false);

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadReceipt(id);
    } else {
      this.router.navigate(['/admin/inventory-receipts']);
    }
  }

  loadReceipt(id: string) {
    this.isLoading.set(true);
    this.inventoryService.getReceiptById(id).subscribe({
      next: (data) => {
        this.receipt.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Lỗi khi tải chi tiết phiếu nhập', err);
        alert('Không tìm thấy phiếu nhập!');
        this.router.navigate(['/admin/inventory-receipts']);
      }
    });
  }

  completeReceipt() {
    if (confirm('Xác nhận nhập kho? Số lượng tồn kho của sản phẩm sẽ được cộng thêm.')) {
      const id = this.receipt()?.receiptId;
      if (!id) return;
      
      this.isProcessing.set(true);
      this.inventoryService.completeReceipt(id).subscribe({
        next: (data) => {
          this.receipt.set(data);
          this.isProcessing.set(false);
          alert('Nhập kho thành công!');
        },
        error: (err) => {
          console.error('Lỗi khi nhập kho', err);
          alert('Có lỗi xảy ra: ' + (err.error?.message || err.message));
          this.isProcessing.set(false);
        }
      });
    }
  }

  openCancelModal() {
    this.cancelReason.set('');
    this.showCancelModal.set(true);
  }

  closeCancelModal() {
    this.showCancelModal.set(false);
  }

  cancelReceipt() {
    if (!this.cancelReason().trim()) {
      alert('Vui lòng nhập lý do hủy');
      return;
    }

    const id = this.receipt()?.receiptId;
    if (!id) return;

    this.isProcessing.set(true);
    this.inventoryService.cancelReceipt(id, this.cancelReason()).subscribe({
      next: (data) => {
        this.receipt.set(data);
        this.closeCancelModal();
        this.isProcessing.set(false);
        alert('Đã hủy phiếu nhập!');
      },
      error: (err) => {
        console.error('Lỗi khi hủy phiếu', err);
        alert('Có lỗi xảy ra: ' + (err.error?.message || err.message));
        this.isProcessing.set(false);
      }
    });
  }
}
