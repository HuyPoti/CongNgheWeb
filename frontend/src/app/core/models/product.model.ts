// ==========================================
// DTOs khớp với backend (camelCase JSON)
// ==========================================

export interface ProductListItemDto {
  id: string;
  name: string;
  price: number;
  categoryName: string;
  thumbnailUrl: string | null;
}

export interface ProductDto {
  productId: string;
  name: string;
  slug: string;
  sku: string | null;
  regularPrice: number;
  salePrice: number | null;
  stockQuantity: number;
  warrantyMonths: number;
  categoryName: string;
  description: string | null;
  specifications: string | null;
  status: number;
  brandId: string;
  createdAt: string;
}

export interface ProductImageDto {
  productImageId: string;
  imageUrl: string;
  isPrimary: boolean;
  sortOrder: number;
}

export interface ProductSpecDto {
  specId: string;
  specKey: string;
  specValue: string;
}

export interface ProductFullDto {
  product: ProductDto;
  images: ProductImageDto[];
  specs: ProductSpecDto[];
}

export interface PagedProductResponse {
  items: ProductDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface ProductListResponse {
  items: ProductListItemDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

// ==========================================
// Request DTOs gửi lên backend
// ==========================================

export interface CreateProductDto {
  categoryId: string;
  name: string;
  slug: string;
  sku?: string;
  regularPrice: number;
  salePrice?: number | null;
  stockQuantity: number;
  warrantyMonths: number;
  description?: string;
  specifications?: string;
  status?: number;
  brandId: string;
}

export interface UpdateProductDto {
  name?: string;
  slug?: string;
  sku?: string;
  categoryId?: string;
  regularPrice?: number;
  salePrice?: number | null;
  stockQuantity?: number;
  warrantyMonths?: number;
  description?: string;
  specifications?: string;
  status?: number;
  brandId?: string;
}

export interface CreateProductImageDto {
  imageUrl: string;
  isPrimary: boolean;
  sortOrder: number;
}

export interface CreateProductSpecDto {
  specKey: string;
  specValue: string;
}

// ==========================================
// UI Models (dùng nội bộ frontend)
// ==========================================

export interface ProductCard {
  id: string;
  name: string;
  price: number;
  image: string;
  category: string;
  specs: Record<string, string>;
  brand?: string;
}

export interface Review {
  id: string;
  userName: string;
  userInitial: string;
  rating: number;
  date: string;
  title: string;
  body: string;
  product: string;
  verified: boolean;
  helpful: number;
  images: string[];
}
