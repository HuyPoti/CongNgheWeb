import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReviewService } from '../../../core/services/review.service';
import { UserService } from '../../../core/services/user.service';
import { ReviewDto } from '../../../core/models/review.model';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-emp-reviews',
  standalone: true,
  imports: [CommonModule, FormsModule, DatePipe],
  templateUrl: './emp-reviews.html',
})
export class EmpReviews implements OnInit {
  private reviewService = inject(ReviewService);
  private userService = inject(UserService);
  private toast = inject(ToastService);

  reviews = signal<ReviewDto[]>([]);
  isLoading = signal(false);

  // Reply form states
  replyingToId = signal<string | null>(null);
  replyContent = signal('');
  isSubmittingReply = signal(false);

  // Staff user ID (lấy từ DB)
  staffUserId = signal<string | null>(null);

  // Filter
  searchQuery = signal('');
  ratingFilter = signal<number | null>(null);

  ngOnInit(): void {
    this.loadReviews();
    this.loadStaffUser();
  }

  loadStaffUser(): void {
    this.userService.getAll().subscribe({
      next: (users) => {
        // Tìm user staff hoặc admin để reply
        const staff = users.find((u) => u.role === 'staff' || u.role === 'admin');
        if (staff) {
          this.staffUserId.set(staff.userId);
        } else {
          this.toast.error('Không tìm thấy tài khoản nhân viên');
        }
      },
      error: () => {
        this.toast.error('Lỗi khi tải thông tin nhân viên');
      },
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
        this.toast.error('Lỗi khi tải danh sách đánh giá');
      },
    });
  }

  get filteredReviews(): ReviewDto[] {
    let result = this.reviews();
    const q = this.searchQuery().toLowerCase();
    const rating = this.ratingFilter();

    if (q) {
      result = result.filter(
        (r) =>
          r.productName.toLowerCase().includes(q) ||
          r.userName.toLowerCase().includes(q) ||
          r.comment?.toLowerCase().includes(q),
      );
    }

    if (rating) {
      result = result.filter((r) => r.rating === rating);
    }

    return result;
  }

  get reviewStats() {
    const all = this.reviews();
    return {
      total: all.length,
      avgRating: all.length
        ? (all.reduce((sum, r) => sum + r.rating, 0) / all.length).toFixed(1)
        : '0',
      needReply: all.filter((r) => !r.replies || r.replies.length === 0).length,
      replied: all.filter((r) => r.replies && r.replies.length > 0).length,
    };
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
    const staffId = this.staffUserId();
    if (!this.replyContent().trim() || !staffId) {
      if (!staffId) this.toast.error('Đang xác thực quyền nhân viên...');
      return;
    }

    this.isSubmittingReply.set(true);
    this.reviewService
      .createReply(reviewId, {
        userId: staffId,
        content: this.replyContent(),
      })
      .subscribe({
        next: (reply) => {
          this.reviews.update((list) =>
            list.map((r) => {
              if (r.reviewId === reviewId) {
                return { ...r, replies: [...(r.replies || []), reply] };
              }
              return r;
            }),
          );
          this.toast.success('Đã gửi phản hồi');
          this.replyingToId.set(null);
          this.replyContent.set('');
          this.isSubmittingReply.set(false);
        },
        error: (err) => {
          this.toast.error('Lỗi khi gửi phản hồi: ' + (err.error?.message || 'Không xác định'));
          this.isSubmittingReply.set(false);
        },
      });
  }

  getRatingStars(rating: number): number[] {
    return Array(rating).fill(0);
  }

  getEmptyStars(rating: number): number[] {
    return Array(5 - rating).fill(0);
  }
}
