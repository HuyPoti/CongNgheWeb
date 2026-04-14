import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReviewService } from '../../../core/services/review.service';
import { UserService } from '../../../core/services/user.service';
import { ReviewDto } from '../../../core/models/review.model';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-admin-reviews',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe],
  templateUrl: './reviews.html',
})
export class AdminReviewsComponent implements OnInit {
  private reviewService = inject(ReviewService);
  private userService = inject(UserService);
  private toast = inject(ToastService);
  
  reviews = signal<ReviewDto[]>([]);
  isLoading = signal(false);

  // States for custom confirmation modal
  showDeleteModal = signal(false);
  reviewIdToDelete = signal<string | null>(null);

  // States for reply form
  replyingToId = signal<string | null>(null);
  replyContent = signal('');
  isSubmittingReply = signal(false);

  // ID của Admin sẽ được lấy năng động từ database
  adminUserId = signal<string | null>(null);

  ngOnInit(): void {
    this.loadReviews();
    this.loadAdminUser();
  }

  loadAdminUser(): void {
    this.userService.getAll().subscribe({
      next: (users) => {
        // Tìm người dùng đầu tiên có quyền admin trong database
        const admin = users.find(u => u.role === 'admin');
        if (admin) {
          this.adminUserId.set(admin.userId);
        } else {
          this.toast.error('Hệ thống chưa có tài khoản Admin để thực hiện phản hồi.');
        }
      },
      error: () => {
        this.toast.error('Lỗi khi kiểm tra danh sách người dùng');
      }
    });
  }

  loadReviews(): void {
    this.isLoading.set(true);
    this.reviewService.getAll().subscribe({
      next: (reviews) => {
        this.reviews.set(reviews);
        this.isLoading.set(false);
      },
      error: () => {
        this.reviews.set([]);
        this.isLoading.set(false);
        this.toast.error('Lỗi khi tải danh sách review');
      },
    });
  }

  get activeReviewsCount() {
    return this.reviews().filter((r) => r.isActive === 1).length;
  }

  toggleActive(review: ReviewDto): void {
    const newActive = review.isActive === 1 ? 2 : 1; 
    this.reviewService.updateActive(review.reviewId, { isActive: newActive }).subscribe({
      next: (updated: ReviewDto) => {
        this.reviews.update(list => 
          list.map(r => r.reviewId === updated.reviewId ? { ...updated, replies: r.replies } : r)
        );
        this.toast.success(newActive === 1 ? 'Đã hiển thị đánh giá' : 'Đã ẩn đánh giá');
      }
    });
  }

  deleteReview(id: string): void {
    this.reviewIdToDelete.set(id);
    this.showDeleteModal.set(true);
  }

  cancelDelete(): void {
    this.showDeleteModal.set(false);
    this.reviewIdToDelete.set(null);
  }

  confirmDelete(): void {
    const id = this.reviewIdToDelete();
    if (!id) return;

    this.showDeleteModal.set(false);
    this.isLoading.set(true);

    this.reviewService.delete(id).subscribe({
      next: () => {
        this.reviews.update((reviews) => reviews.filter((r) => r.reviewId !== id));
        this.toast.success('Đã xóa đánh giá vĩnh viễn');
        this.isLoading.set(false);
        this.reviewIdToDelete.set(null);
      },
      error: (err) => {
        this.toast.error('Lỗi khi xóa đánh giá: ' + (err.error?.message || 'Không xác định'));
        this.isLoading.set(false);
        this.reviewIdToDelete.set(null);
      },
    });
  }

  toggleReplyForm(reviewId: string): void {
    if (this.replyingToId() === reviewId) {
      this.replyingToId.set(null);
      this.replyContent.set('');
    } else {
      this.replyingToId.set(reviewId);
      this.replyContent.set('');
    }
  }

  submitReply(reviewId: string): void {
    const adminId = this.adminUserId();
    if (!this.replyContent().trim() || !adminId) {
      if (!adminId) this.toast.error('Vui lòng đợi hệ thống xác thực quyền Admin');
      return;
    }

    this.isSubmittingReply.set(true);
    this.reviewService.createReply(reviewId, {
      userId: adminId,
      content: this.replyContent()
    }).subscribe({
      next: (reply) => {
        this.reviews.update(list => 
          list.map(r => {
            if (r.reviewId === reviewId) {
              return { ...r, replies: [...(r.replies || []), reply] };
            }
            return r;
          })
        );
        this.toast.success('Đã gửi phản hồi');
        this.replyingToId.set(null);
        this.replyContent.set('');
        this.isSubmittingReply.set(false);
      },
      error: (err) => {
        console.log(err);
        this.toast.error('Lỗi khi gửi phản hồi: ' + (err.error?.message || 'Không xác định'));
        this.isSubmittingReply.set(false);
      }
    });
  }

  deleteReply(reviewId: string, replyId: string): void {
    if (!confirm('Bạn có chắc muốn xóa phản hồi này?')) return;

    this.reviewService.deleteReply(replyId).subscribe({
      next: () => {
        this.reviews.update(list => 
          list.map(r => {
            if (r.reviewId === reviewId) {
              return { ...r, replies: r.replies.filter(rp => rp.replyId !== replyId) };
            }
            return r;
          })
        );
        this.toast.success('Đã xóa phản hồi');
      }
    });
  }

  getRatingStars(rating: number): number[] {
    return Array(rating).fill(0);
  }
}

