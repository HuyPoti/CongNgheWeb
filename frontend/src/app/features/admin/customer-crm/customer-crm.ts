// Component quản lý người dùng - ĐÃ KẾT NỐI API THẬT
// Thay thế data mock bằng gọi API từ UserService

import { Component, signal, inject, OnInit, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../../core/services/user.service';
import { ToastService } from '../../../core/services/toast.service';
import { AuthService } from '../../../core/services/auth.service';
import { ConfirmService } from '../../../core/services/confirm.service';
import { User, UpdateUser } from '../../../core/models/user.model';

@Component({
  selector: 'app-customer-crm',
  imports: [CommonModule, FormsModule],
  templateUrl: './customer-crm.html',
  styles: ``,
})
export class CustomerCrm implements OnInit {
  private userService = inject(UserService);
  private toastService = inject(ToastService);
  private platformId = inject(PLATFORM_ID);
  private authService = inject(AuthService);
  private confirmService = inject(ConfirmService);

  users = signal<User[]>([]);        // ← Khởi tạo rỗng, sẽ load từ API
  isLoading = signal(true);          // ← Trạng thái loading
  errorMessage = signal('');         // ← Thông báo lỗi

  showModal = signal(false);
  editingUser = signal<User | null>(null);
  form: Partial<User> = {};
  searchQuery = '';

  roleLabels: Record<string, string> = {
    customer: 'Khách hàng',
    admin: 'Quản trị',
    staff: 'Nhân viên',
  };

  // OnInit = lifecycle hook, chạy sau khi component được tạo
  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.loadUsers();  // ← Gọi API lấy danh sách users khi component load ở browser
    }
  }

  loadUsers() {
    this.isLoading.set(true);
    this.userService.getAll().subscribe({
      next: (data) => {                     // ← Thành công
        this.users.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {                     // ← Thất bại
        console.error('Lỗi tải users:', err);
        this.errorMessage.set('Không thể tải danh sách người dùng');
        this.isLoading.set(false);
      },
    });
  }

  get filteredUsers() {
    const q = this.searchQuery.toLowerCase();
    const customers = this.users().filter(u => u.role === 'customer');
    
    if (!q) return customers;
    return customers.filter(
      (u) =>
        u.fullName.toLowerCase().includes(q) ||
        u.email.toLowerCase().includes(q) ||
        (u.phone && u.phone.includes(q))
    );
  }

  // Mở modal SỬA
  openEdit(user: User) {
    this.editingUser.set(user);
    this.form = { ...user };
    this.showModal.set(true);
  }

  closeModal() {
    this.showModal.set(false);
  }

  save() {
    // === VALIDATION TRƯỚC KHI GỬI ===
    if (!this.form.fullName?.trim()) {
      this.toastService.warning('Vui lòng điền Họ tên');
      return; 
    }

    if (this.form.phone?.trim()) {
      const phoneRegex = /^(0[23456789][0-9]{8})$/;
      if (!phoneRegex.test(this.form.phone.trim())) {
        this.toastService.warning('Số điện thoại không hợp lệ (phải gồm 10 chữ số bắt đầu bằng số 0)');
        return;
      }
    }

    // CẬP NHẬT → PUT /api/users/:id
    const userId = this.editingUser()?.userId;
    if (!userId) return;
    const updateData: UpdateUser = {
      fullName: this.form.fullName,
      phone: this.form.phone || '',
    };
    
    this.userService.update(userId, updateData).subscribe({
      next: () => {
        this.toastService.success('Cập nhật thành công');
        this.loadUsers();
        this.closeModal();
        this.errorMessage.set('');
      },
      error: (err) => {
        let msg = 'Lỗi cập nhật';
        if (err.error?.message) {
          msg = err.error.message;
        } else if (err.error?.errors) {
          const keys = Object.keys(err.error.errors);
          if (keys.length > 0) msg = err.error.errors[keys[0]][0];
        }

        this.errorMessage.set(msg);
        this.toastService.error(msg);
      },
    });
  }

  async toggleActive(user: User) {
    const currentUserId = this.authService.currentUserValue?.userId;
    if (user.userId === currentUserId) {
      this.toastService.warning('Bạn không thể tự khóa tài khoản của chính mình!');
      return;
    }
    const action = user.isActive ? 'Khóa' : 'Kích hoạt';
    const isConfirmed = await this.confirmService.confirm(
      `Bạn có chắc chắn muốn ${action.toLowerCase()} tài khoản "${user.fullName}"?`,
      `${action} tài khoản`,
      user.isActive ? 'danger' : 'info'
    );
    if (isConfirmed) {
      this.userService.update(user.userId, { isActive: !user.isActive }).subscribe({
        next: () => {
          this.toastService.success(`${action} tài khoản thành công`);
          this.loadUsers();
        },
        error: () => this.toastService.error(`Lỗi khi ${action.toLowerCase()} tài khoản`),
      });
    }
  }

  getUserCountByRole(role: string) {
    return this.users().filter((u) => u.role === role).length;
  }
}
