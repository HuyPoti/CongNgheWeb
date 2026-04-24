// ================================================================
// DTOs – khop 1-1 voi backend JSON response (camelCase)
// ================================================================

export interface ProductListItemDto {
  id: string;
  name: string;
  slug: string;
  // price = SalePrice ?? RegularPrice (gia ban thuc te)
  price: number;
  regularPrice: number;
  salePrice: number | null;
  categoryName: string;
  thumbnailUrl: string | null;
  brandName: string;
  brandId: string;
  stockQuantity: number;
  warrantyMonths: number;
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
  imageId: string;
  imageUrl: string;
  isPrimary: boolean;
  sortOrder: number;
}



export interface ProductFullDto {
  product: ProductDto;
  images: ProductImageDto[];
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

// ================================================================
// Request DTOs gui len backend
// ================================================================

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



// ================================================================
// Options object cho fetchClientProducts (thay the overload cu)
// ================================================================

export interface ClientProductsQuery {
  page?: number;
  pageSize?: number;
  categorySlug?: string;
  keyword?: string;
  brandId?: string;
  minPrice?: number;
  maxPrice?: number;
  sortBy?: 'newest' | 'price_asc' | 'price_desc' | 'name_asc';
}

// ================================================================
// UI Models – chi dung noi bo trong component/template
// ================================================================

export interface ProductCard {
  id: string;
  name: string;
  slug: string;
  price: number;
  regularPrice: number;
  salePrice: number | null;
  image: string;
  category: string;
  brand: string;
  brandId: string;
  stockQuantity: number;
  warrantyMonths: number;
  specs: Record<string, string>;
}

// Giu lai de tuong thich voi cac component cu (reviews, comparison)
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
