import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReturnRequestService, ReturnRequest } from '../../../core/services/return-request.service';
import { ToastService } from '../../../core/services/toast.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-admin-return-requests',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './return-requests.html',
})
export class AdminReturnRequestsComponent implements OnInit {
  private returnService = inject(ReturnRequestService);
  private toast = inject(ToastService);

  requests = signal<ReturnRequest[]>([]);
  isLoading = signal(true);
  selectedRequest = signal<ReturnRequest | null>(null);

  processingStatus = 'Approved';
  refundAmount = 0;
  adminNote = '';
  isSubmitting = signal(false);

  ngOnInit(): void {
    this.loadRequests();
  }

  loadRequests(): void {
    this.isLoading.set(true);
    this.returnService.getAll().subscribe({
      next: (data) => {
        this.requests.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        // Không hiện toast lỗi nếu là lỗi 401 hoặc 403 vì interceptor/guard đã lo việc chuyển hướng đăng nhập
        if (err?.status !== 401 && err?.status !== 403) {
          this.toast.error('Không thể tải danh sách yêu cầu đổi trả');
        }
        this.isLoading.set(false);
      }
    });
  }

  selectRequest(req: ReturnRequest): void {
    this.selectedRequest.set(req);
    this.refundAmount = req.refundAmount || 0;
    this.adminNote = req.adminNote || '';
    this.processingStatus = req.status === 'pending' ? 'Approved' : this.capitalizeFirst(req.status);
  }

  updateStatus(): void {
    const current = this.selectedRequest();
    if (!current) return;

    // Validation
    if (this.processingStatus === 'Approved' || this.processingStatus === 'Completed') {
      if (this.refundAmount < 0) {
        this.toast.error('Số tiền hoàn lại không được âm');
        return;
      }
    }
    if (this.processingStatus === 'Rejected' && !this.adminNote.trim()) {
      this.toast.error('Vui lòng nhập lý do từ chối trong phần Ghi chú');
      return;
    }

    this.isSubmitting.set(true);
    this.returnService.process(current.returnId, {
      status: this.processingStatus.toLowerCase(),
      refundAmount: this.refundAmount,
      adminNote: this.adminNote
    }).subscribe({
      next: (updated) => {
        this.toast.success('Cập nhật trạng thái thành công');
        this.isSubmitting.set(false);
        this.loadRequests();
        this.selectedRequest.set(updated);
      },
      error: (err) => {
        this.toast.error(err?.error?.message || 'Có lỗi xảy ra khi cập nhật');
        this.isSubmitting.set(false);
      }
    });
  }

  private capitalizeFirst(s: string): string {
    return s ? s.charAt(0).toUpperCase() + s.slice(1).toLowerCase() : '';
  }

  getStatusClass(status: string): string {
    switch (status.toLowerCase()) {
      case 'pending': return 'bg-amber-500/10 text-amber-500 border-amber-500/20';
      case 'approved': return 'bg-emerald-500/10 text-emerald-500 border-emerald-500/20';
      case 'rejected': return 'bg-rose-500/10 text-rose-500 border-rose-500/20';
      case 'completed': return 'bg-sky-500/10 text-sky-500 border-sky-500/20';
      default: return 'bg-slate-500/10 text-slate-500 border-slate-500/20';
    }
  }

  getStatusLabel(status: string): string {
    switch (status.toLowerCase()) {
      case 'pending': return 'Đang chờ';
      case 'approved': return 'Đã duyệt';
      case 'rejected': return 'Từ chối';
      case 'completed': return 'Hoàn tất';
      default: return status;
    }
  }
}
