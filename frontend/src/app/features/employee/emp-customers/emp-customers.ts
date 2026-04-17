import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../../core/services/user.service';
import { OrderService } from '../../../core/services/order.service';
import { ToastService } from '../../../core/services/toast.service';
import { User } from '../../../core/models/user.model';
import { OrderDto } from '../../../core/models/order.model';

@Component({
  selector: 'app-emp-customers',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './emp-customers.html',
})
export class EmpCustomers implements OnInit {
  private userService = inject(UserService);
  private orderService = inject(OrderService);
  private toast = inject(ToastService);

  // States
  users = signal<User[]>([]);
  isLoading = signal(true);
  searchQuery = signal('');

  // Customer detail
  selectedCustomer = signal<User | null>(null);
  customerOrders = signal<OrderDto[]>([]);
  isLoadingOrders = signal(false);

  ngOnInit() {
    this.loadUsers();
  }

  loadUsers() {
    this.isLoading.set(true);
    this.userService.getAll().subscribe({
      next: (data) => {
        this.users.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.toast.error('Không thể tải danh sách khách hàng');
        this.isLoading.set(false);
      },
    });
  }

  get filteredCustomers(): User[] {
    const q = this.searchQuery().toLowerCase();
    const customers = this.users().filter((u) => u.role === 'customer');
    if (!q) return customers;
    return customers.filter(
      (u) =>
        u.fullName.toLowerCase().includes(q) ||
        u.email.toLowerCase().includes(q) ||
        (u.phone && u.phone.includes(q)),
    );
  }

  get customerStats() {
    const customers = this.users().filter((u) => u.role === 'customer');
    return {
      total: customers.length,
      thisMonth: customers.filter((u) => {
        const created = new Date(u.createdAt);
        const now = new Date();
        return created.getMonth() === now.getMonth() && created.getFullYear() === now.getFullYear();
      }).length,
    };
  }

  openCustomerDetail(customer: User) {
    this.selectedCustomer.set(customer);
    this.isLoadingOrders.set(true);

    // Load orders for this customer
    this.orderService.getAll(undefined, 1, 50).subscribe({
      next: (result) => {
        // Filter orders by customer name (API hiện tại chưa hỗ trợ filter by userId)
        this.customerOrders.set(result.items.filter((o) => o.customerName === customer.fullName));
        this.isLoadingOrders.set(false);
      },
      error: () => {
        this.customerOrders.set([]);
        this.isLoadingOrders.set(false);
      },
    });
  }

  closeDetail() {
    this.selectedCustomer.set(null);
    this.customerOrders.set([]);
  }

  getCustomerOrderTotal(orders: OrderDto[]): number {
    return orders.reduce((sum, o) => sum + o.totalAmount, 0);
  }

  statusLabels: Record<string, string> = {
    pending: 'Chờ xử lý',
    confirmed: 'Đã xác nhận',
    processing: 'Đang xử lý',
    shipping: 'Đang giao',
    delivered: 'Đã giao',
    cancelled: 'Đã hủy',
  };

  statusColors: Record<string, string> = {
    pending: 'bg-yellow-500/20 text-yellow-400 border-yellow-500/30',
    confirmed: 'bg-blue-500/20 text-blue-400 border-blue-500/30',
    processing: 'bg-purple-500/20 text-purple-400 border-purple-500/30',
    shipping: 'bg-cyan-500/20 text-cyan-400 border-cyan-500/30',
    delivered: 'bg-emerald-500/20 text-emerald-400 border-emerald-500/30',
    cancelled: 'bg-red-500/20 text-red-400 border-red-500/30',
  };
}
