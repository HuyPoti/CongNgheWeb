import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ReviewDto,
  UpdateReviewActiveDto,
  ReviewReplyDto,
  CreateReviewReplyDto,
  UpdateReviewReplyDto,
  ReviewImageDto,
  CreateReviewImageDto,
  ToggleVoteDto,
  ToggleVoteResponse,
} from '../models/review.model';

@Injectable({
  providedIn: 'root',
})
export class ReviewService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/reviews`;

  // ============================
  // REVIEWS
  // ============================

  getAll(): Observable<ReviewDto[]> {
    return this.http.get<ReviewDto[]>(this.baseUrl);
  }

  getByProductId(productId: string): Observable<ReviewDto[]> {
    return this.http.get<ReviewDto[]>(`${this.baseUrl}/product/${productId}`);
  }

  getById(id: string): Observable<ReviewDto> {
    return this.http.get<ReviewDto>(`${this.baseUrl}/${id}`);
  }

  updateActive(id: string, dto: UpdateReviewActiveDto): Observable<ReviewDto> {
    return this.http.patch<ReviewDto>(`${this.baseUrl}/${id}/active`, dto);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  // ============================
  // REVIEW REPLIES
  // ============================

  createReply(reviewId: string, dto: CreateReviewReplyDto): Observable<ReviewReplyDto> {
    return this.http.post<ReviewReplyDto>(`${this.baseUrl}/${reviewId}/replies`, dto);
  }

  updateReply(replyId: string, dto: UpdateReviewReplyDto): Observable<ReviewReplyDto> {
    return this.http.put<ReviewReplyDto>(`${this.baseUrl}/replies/${replyId}`, dto);
  }

  deleteReply(replyId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/replies/${replyId}`);
  }

  // ============================
  // REVIEW IMAGES
  // ============================

  addImage(reviewId: string, dto: CreateReviewImageDto): Observable<ReviewImageDto> {
    return this.http.post<ReviewImageDto>(`${this.baseUrl}/${reviewId}/images`, dto);
  }

  deleteImage(imageId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/images/${imageId}`);
  }

  getImages(reviewId: string): Observable<ReviewImageDto[]> {
    return this.http.get<ReviewImageDto[]>(`${this.baseUrl}/${reviewId}/images`);
  }

  // ============================
  // REVIEW HELPFUL VOTES
  // ============================

  toggleVote(reviewId: string, dto: ToggleVoteDto): Observable<ToggleVoteResponse> {
    return this.http.post<ToggleVoteResponse>(`${this.baseUrl}/${reviewId}/votes/toggle`, dto);
  }

  getVoteCount(reviewId: string): Observable<{ helpfulCount: number }> {
    return this.http.get<{ helpfulCount: number }>(`${this.baseUrl}/${reviewId}/votes/count`);
  }

  checkUserVoted(reviewId: string, userId: string): Observable<{ hasVoted: boolean }> {
    return this.http.get<{ hasVoted: boolean }>(
      `${this.baseUrl}/${reviewId}/votes/check/${userId}`,
    );
  }

  createReview(dto: {
    productId: string;
    userId: string;
    rating: number;
    comment: string;
    isVerifiedPurchase: boolean;
  }): Observable<ReviewDto> {
    return this.http.post<ReviewDto>(this.baseUrl, dto);
  }
}
