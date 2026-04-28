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
  shippingFee: number;
  discountAmount: number;
  userId: string;
  shippingAddress: AddressDto;
  items: OrderItemDto[];
  payment?: PaymentDto;
  shipment?: ShipmentDto;
  statusHistory: OrderStatusHistoryDto[];
  returnRequest?: ReturnRequestDto;
}

export interface PaymentDto {
  paymentId: string;
  paymentMethod: string;
  amount: number;
  status: string;
  createdAt: string;
}

export interface ShipmentDto {
  shipmentId: string;
  carrier: string;
  trackingCode?: string;
  status: string;
  estimatedDelivery?: string;
  actualDelivery?: string;
}

export interface OrderStatusHistoryDto {
  id: string;
  oldStatus: number;
  newStatus: number;
  note?: string;
  createdAt: string;
}

export interface ReturnRequestDto {
  returnId: string;
  reason: string;
  status: string;
  createdAt: string;
}

export interface WishlistItemDto {
  wishlistId: string;
  productId: string;
  productName: string;
  productImageUrl: string;
  price: number;
  createdAt: string;
}

export interface UpdateOrderDto {
  status?: 'pending' | 'confirmed' | 'processing' | 'shipping' | 'delivered' | 'cancelled';
  paymentStatus?: 'unpaid' | 'paid' | 'refunded';
  cancelledReason?: string;
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
  couponCode?: string;
  shippingFee: number;
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
