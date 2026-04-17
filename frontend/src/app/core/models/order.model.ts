export interface OrderDto {
  orderId: string;
  orderCode: string;
  status: 'pending' | 'confirmed' | 'processing' | 'shipping' | 'delivered' | 'cancelled';
  paymentMethod: string;
  paymentStatus: 'unpaid' | 'paid' | 'refunded';
  totalAmount: number;
  customerName: string;
  createdAt: string;
  updatedAt: string;
}

export interface AddressDto {
  addressId: string;
  recipientName: string;
  phone: string;
  addressLine: string;
  province: string;
  district: string;
  ward: string;
}

export interface OrderItemDto {
  orderItemId: string;
  productId: string;
  productName: string;
  productImageUrl: string | null;
  quantity: number;
  unitPrice: number;
}

export interface OrderDetailDto extends OrderDto {
  notes: string | null;
  userId: string;
  shippingAddress: AddressDto;
  items: OrderItemDto[];
}

export interface UpdateOrderDto {
  status?: 'pending' | 'confirmed' | 'processing' | 'shipping' | 'delivered' | 'cancelled';
  paymentStatus?: 'unpaid' | 'paid' | 'refunded';
}

export interface CreateOrderItemDto {
  productId: string;
  quantity: number;
}

export interface CreateOrderDto {
  userId?: string;
  shippingAddressId?: string;
  shippingAddress?: CreateOrderShippingAddressDto;
  paymentMethod: string;
  notes?: string;
  items: CreateOrderItemDto[];
}

export interface CreateOrderShippingAddressDto {
  recipientName: string;
  phone: string;
  addressLine: string;
  province?: string;
  district?: string;
  ward?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface ApiResponse {
  message: string;
}
