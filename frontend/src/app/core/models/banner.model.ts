export type BannerPosition =
  | 'homepage_slider'
  | 'homepage_mid_top_right'
  | 'homepage_mid_bottom_right'
  | 'homepage_mid_wide';

export interface Banner {
  bannerId: string;
  title: string | null;
  subtitle: string | null;
  imageUrl: string;
  linkUrl: string | null;
  position: BannerPosition;
  sortOrder: number;
  isActive: boolean;
  startDate: string | null;
  endDate: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface CreateBanner {
  title: string | null;
  subtitle?: string | null;
  imageUrl: string | null;
  linkUrl?: string | null;
  position: BannerPosition;
  sortOrder: number;
  isActive: boolean;
  startDate?: string | null;
  endDate?: string | null;
}

export interface UpdateBanner {
  title?: string | null;
  subtitle?: string | null;
  imageUrl?: string | null;
  linkUrl?: string | null;
  position?: BannerPosition;
  sortOrder?: number;
  isActive?: boolean;
  startDate?: string | null;
  endDate?: string | null;
}