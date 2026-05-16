import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
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
import { ApiResponse } from '../models/api-response.model';

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
    return this.http
      .get<ApiResponse<{ items: ReviewDto[] }>>(this.baseUrl)
      .pipe(map((res) => res.data.items));
  }

  getByProductId(productId: string): Observable<ReviewDto[]> {
    return this.http
      .get<ApiResponse<{ items: ReviewDto[] }>>(`${this.baseUrl}/product/${productId}`)
      .pipe(map((res) => res.data.items));
  }

  getById(id: string): Observable<ReviewDto> {
    return this.http.get<ApiResponse<ReviewDto>>(`${this.baseUrl}/${id}`).pipe(map((res) => res.data));
  }

  updateActive(id: string, dto: UpdateReviewActiveDto): Observable<ReviewDto> {
    return this.http
      .patch<ApiResponse<ReviewDto>>(`${this.baseUrl}/${id}/active`, dto)
      .pipe(map((res) => res.data));
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  // ============================
  // REVIEW REPLIES
  // ============================

  createReply(reviewId: string, dto: CreateReviewReplyDto): Observable<ReviewReplyDto> {
    return this.http
      .post<ApiResponse<ReviewReplyDto>>(`${this.baseUrl}/${reviewId}/replies`, dto)
      .pipe(map((res) => res.data));
  }

  updateReply(replyId: string, dto: UpdateReviewReplyDto): Observable<ReviewReplyDto> {
    return this.http
      .put<ApiResponse<ReviewReplyDto>>(`${this.baseUrl}/replies/${replyId}`, dto)
      .pipe(map((res) => res.data));
  }

  deleteReply(replyId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/replies/${replyId}`);
  }

  // ============================
  // REVIEW IMAGES
  // ============================

  addImage(reviewId: string, dto: CreateReviewImageDto): Observable<ReviewImageDto> {
    return this.http
      .post<ApiResponse<ReviewImageDto>>(`${this.baseUrl}/${reviewId}/images`, dto)
      .pipe(map((res) => res.data));
  }

  deleteImage(imageId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/images/${imageId}`);
  }

  getImages(reviewId: string): Observable<ReviewImageDto[]> {
    return this.http
      .get<ApiResponse<ReviewImageDto[]>>(`${this.baseUrl}/${reviewId}/images`)
      .pipe(map((res) => res.data));
  }

  // ============================
  // REVIEW HELPFUL VOTES
  // ============================

  toggleVote(reviewId: string, dto: ToggleVoteDto): Observable<ToggleVoteResponse> {
    return this.http
      .post<ApiResponse<ToggleVoteResponse>>(`${this.baseUrl}/${reviewId}/votes/toggle`, dto)
      .pipe(map((res) => res.data));
  }

  getVoteCount(reviewId: string): Observable<{ helpfulCount: number }> {
    return this.http
      .get<ApiResponse<{ helpfulCount: number }>>(`${this.baseUrl}/${reviewId}/votes/count`)
      .pipe(map((res) => res.data));
  }

  checkUserVoted(reviewId: string, userId: string): Observable<{ hasVoted: boolean }> {
    return this.http
      .get<ApiResponse<{ hasVoted: boolean }>>(
        `${this.baseUrl}/${reviewId}/votes/check/${userId}`,
      )
      .pipe(map((res) => res.data));
  }

  createReview(dto: {
    productId: string;
    userId: string;
    rating: number;
    comment: string;
    isVerifiedPurchase: boolean;
  }): Observable<ReviewDto> {
    return this.http.post<ApiResponse<ReviewDto>>(this.baseUrl, dto).pipe(map((res) => res.data));
  }
}
