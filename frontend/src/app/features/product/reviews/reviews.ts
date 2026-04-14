import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ReviewService } from '../../../core/services/review.service';
import { ReviewDto } from '../../../core/models/review.model';

@Component({
  selector: 'app-reviews',
  standalone: true,
  imports: [RouterLink, CommonModule],
  templateUrl: './reviews.html',
  styleUrl: './reviews.css',
})
export class Reviews implements OnInit {
  private reviewService = inject(ReviewService);

  isWriteReviewOpen = signal(false);
  selectedRating = signal(0);
  hoverRating = signal(0);
  sortBy = signal('newest');

  averageRating = signal(0);
  totalReviews = signal(0);
  ratingDistribution = signal([
    { stars: 5, count: 0, percentage: 0 },
    { stars: 4, count: 0, percentage: 0 },
    { stars: 3, count: 0, percentage: 0 },
    { stars: 2, count: 0, percentage: 0 },
    { stars: 1, count: 0, percentage: 0 },
  ]);

  reviews = signal<ReviewDto[]>([]);
  isLoading = signal(false);

  // Giả sử user ID cho demo
  currentUserId = '00000000-0000-0000-0000-000000000001';

  ngOnInit(): void {
    this.loadReviews();
  }

  loadReviews(): void {
    this.isLoading.set(true);
    this.reviewService.getAll().subscribe({
      next: (data) => {
        // Chỉ hiện review đang active
        const activeReviews = data.filter(r => r.isActive === 1);
        this.reviews.set(activeReviews);
        this.calculateStats(activeReviews);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  private calculateStats(list: ReviewDto[]): void {
    const total = list.length;
    this.totalReviews.set(total);
    if (total === 0) {
      this.averageRating.set(0);
      return;
    }

    const sum = list.reduce((acc, curr) => acc + curr.rating, 0);
    this.averageRating.set(Number((sum / total).toFixed(1)));

    const distribution = [5, 4, 3, 2, 1].map(star => {
      const count = list.filter(r => r.rating === star).length;
      return {
        stars: star,
        count: count,
        percentage: Math.round((count / total) * 100)
      };
    });
    this.ratingDistribution.set(distribution);
  }

  getStarArray(rating: number): boolean[] {
    return Array.from({ length: 5 }, (_, i) => i < rating);
  }

  toggleWriteReview() {
    this.isWriteReviewOpen.update(v => !v);
    this.selectedRating.set(0);
    this.hoverRating.set(0);
  }

  setRating(rating: number) {
    this.selectedRating.set(rating);
  }

  setHoverRating(rating: number) {
    this.hoverRating.set(rating);
  }

  toggleHelpful(review: ReviewDto): void {
    this.reviewService.toggleVote(review.reviewId, { userId: this.currentUserId }).subscribe({
      next: (res) => {
        // Cập nhật count trực tiếp trong list
        this.reviews.update(list => 
          list.map(r => r.reviewId === review.reviewId ? { ...r, helpfulVoteCount: res.helpfulCount } : r)
        );
      }
    });
  }

  getUserInitial(name: string): string {
    return name ? name.charAt(0).toUpperCase() : '?';
  }

  formatDate(dateStr: string): string {
    return new Date(dateStr).toLocaleDateString('vi-VN', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  }
}

