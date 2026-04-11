export interface NewsCategory {
  categoryId: string; // Đồng bộ với Backend Guid
  name: string;
  slug: string;
  description?: string;
  parentId?: string | null;
  isActive: boolean;
  createdAt?: string;
}

export interface CreateNewsCategory {
  name: string;
  slug: string;
  description?: string;
  parentId?: string | null;
  isActive?: boolean;
}

export type UpdateNewsCategory = Partial<CreateNewsCategory>;

export interface News {
  newId: string; // Tên ID khớp DB (Guid)
  title: string;
  slug: string;
  categoryId: string;
  categoryName?: string;
  content: string;
  excerpt?: string;
  authorId: string;
  imageUrl?: string;
  isActive: boolean;
  isPublished: boolean;
  publishedAt?: string | null;
  views: number;
  metaTitle?: string;
  metaDescription?: string;
}

export interface CreateNews {
  title: string;
  slug: string;
  categoryId: string;
  content: string;
  excerpt?: string;
  imageUrl?: string;
  isPublished?: boolean;
  authorId: string;
  metaTitle?: string;
  metaDescription?: string;
}

export interface UpdateNews extends Partial<CreateNews> {
  isActive?: boolean;
}