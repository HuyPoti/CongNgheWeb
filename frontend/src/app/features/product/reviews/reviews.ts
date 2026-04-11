import { Component, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MOCK_REVIEWS } from '../../../core/mocks/product.mock';
import { Review } from '../../../core/models/product.model';

@Component({
  selector: 'app-reviews',
  standalone: true,
  imports: [RouterLink, CommonModule],
  templateUrl: './reviews.html',
  styleUrl: './reviews.css',
})
export class Reviews {
  isWriteReviewOpen = signal(false);
  selectedRating = signal(0);
  hoverRating = signal(0);
  sortBy = signal('newest');

  averageRating = 4.3;
  totalReviews = 120;
  ratingDistribution = [
    { stars: 5, count: 72, percentage: 56 },
    { stars: 4, count: 31, percentage: 24 },
    { stars: 3, count: 15, percentage: 12 },
    { stars: 2, count: 6, percentage: 5 },
    { stars: 1, count: 4, percentage: 3 },
  ];

  reviews = signal<Review[]>(MOCK_REVIEWS);

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
}
