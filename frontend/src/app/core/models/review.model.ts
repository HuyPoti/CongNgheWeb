export interface ReviewImageDto {
  imageId: string;
  imageUrl: string;
  createdAt: string;
}

// === Review Reply ===
export interface ReviewReplyDto {
  replyId: string;
  userId: string;
  fullName: string;
  content: string;
  isActive: number;
  createdAt: string;
  updatedAt: string;
}

// === Review Helpful Vote ===
export interface ReviewHelpfulVoteDto {
  voteId: string;
  userId: string;
  userName: string;
  createdAt: string;
}

// === Review (chính) ===
export interface ReviewDto {
  reviewId: string;
  productId: string;
  productName: string;
  userId: string;
  userName: string;
  rating: number;
  comment: string;
  isActive: number;
  isVerifiedPurchase: boolean;
  createdAt: string;
  updatedAt: string;
  images: ReviewImageDto[];
  replies: ReviewReplyDto[];
  helpfulVotes: ReviewHelpfulVoteDto[];
  helpfulVoteCount: number;
}

export interface UpdateReviewActiveDto {
  isActive: number;
}


// === Create / Update DTOs ===
export interface CreateReviewReplyDto {
  userId: string;
  content: string;
}

export interface UpdateReviewReplyDto {
  content: string;
}

export interface CreateReviewImageDto {
  imageUrl: string;
}

export interface ToggleVoteDto {
  userId: string;
}

// === Response cho vote toggle ===
export interface ToggleVoteResponse {
  helpfulCount: number;
  hasVoted: boolean;
}
