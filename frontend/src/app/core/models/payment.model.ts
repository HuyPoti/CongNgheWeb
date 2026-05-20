export interface PaymentTransaction {
  paymentId: string;
  orderId: string;
  orderCode: string;
  customerName: string;
  amount: number;
  paymentMethod: string;
  transactionId?: string;
  status: number;
  paidAt?: string;
  createdAt: string;
}

export interface PaymentTransactionQuery {
  keyword?: string;
  page?: number;
  pageSize?: number;
}
