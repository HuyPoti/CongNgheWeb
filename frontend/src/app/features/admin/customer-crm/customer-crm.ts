// Component quản lý người dùng - ĐÃ KẾT NỐI API THẬT
// Thay thế data mock bằng gọi API từ UserService

import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../../core/services/user.service';
import { ToastService } from '../../../core/services/toast.service';
import { User, CreateUser, UpdateUser } from '../../../core/models/user.model';

@Component({
  selector: 'app-customer-crm',
  imports: [CommonModule, FormsModule],
  templateUrl: './customer-crm.html',
  styles: ``,
})
export class CustomerCrm implements OnInit {
  private userService = inject(UserService);
  private toastService = inject(ToastService);

  users = signal<User[]>([]);        // ← Khởi tạo rỗng, sẽ load từ API
  isLoading = signal(true);          // ← Trạng thái loading
  errorMessage = signal('');         // ← Thông báo lỗi

  showModal = signal(false);
  isCreating = signal(false);        // ← Phân biệt modal tạo mới / sửa
  editingUser = signal<User | null>(null);
  form: Partial<User & { password?: string }> = {};
  searchQuery = '';

  roleLabels: Record<string, string> = {
    customer: 'Khách hàng',
    admin: 'Quản trị',
    staff: 'Nhân viên',
  };

  // OnInit = lifecycle hook, chạy sau khi component được tạo
  ngOnInit() {
    this.loadUsers();  // ← Gọi API lấy danh sách users khi component load
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
    if (!q) return this.users();
    return this.users().filter(
      (u) =>
        u.fullName.toLowerCase().includes(q) ||
        u.email.toLowerCase().includes(q) ||
        (u.phone && u.phone.includes(q))
    );
  }

  // Mở modal TẠO MỚI
  openCreate() {
    this.isCreating.set(true);
    this.editingUser.set(null);
    this.form = { role: 'customer' };
    this.showModal.set(true);
  }

  // Mở modal SỬA
  openEdit(user: User) {
    this.isCreating.set(false);
    this.editingUser.set(user);
    this.form = { ...user };
    this.showModal.set(true);
  }

  closeModal() {
    this.showModal.set(false);
  }

  save() {
    // === VALIDATION TRƯỚC KHI GỬI ===
    if (!this.form.fullName?.trim() || !this.form.email?.trim()) {
      this.toastService.warning('Vui lòng điền đầy đủ Họ tên và Email');
      return; // Dừng lại, không gọi API
    }

    if (this.isCreating() && !this.form.password?.trim()) {
      this.toastService.warning('Vui lòng nhập mật khẩu cho tài khoản mới');
      return;
    }

    if (this.isCreating()) {
      // TẠO MỚI → POST /api/users
      const createData: CreateUser = {
        email: this.form.email || '',
        password: this.form.password || '',
        fullName: this.form.fullName || '',
        phone: this.form.phone || '', 
        role: this.form.role || 'customer',
      };
      this.userService.create(createData).subscribe({
        next: () => {
          this.toastService.success('Thêm người dùng thành công');
          this.loadUsers();
          this.closeModal();
          this.errorMessage.set('');
        },
        error: (err) => {
          // Xử lý thông minh cho các loại lỗi 400 (Validation) từ Backend
          let msg = 'Lỗi tạo người dùng';
          if (err.error?.message) {
            msg = err.error.message;
          } else if (err.error?.errors) {
            // Lấy lỗi đầu tiên trong mảng lỗi chi tiết
            const keys = Object.keys(err.error.errors);
            if (keys.length > 0) msg = err.error.errors[keys[0]][0];
          } else if (err.error?.title) {
            msg = err.error.title;
          }
          
          this.errorMessage.set(msg);
          this.toastService.error(msg);
        },
      });
    } else {
      // CẬP NHẬT → PUT /api/users/:id
      const userId = this.editingUser()?.userId;
      if (!userId) return;
      const updateData: UpdateUser = {
        email: this.form.email,
        fullName: this.form.fullName,
        phone: this.form.phone || '',
        role: this.form.role,
      };
      this.userService.update(userId, updateData).subscribe({
        next: () => {
          this.toastService.success('Cập nhật thành công');
          this.loadUsers();
          this.closeModal();
          this.errorMessage.set('');
        },
        error: (err) => {
          let msg = 'Lỗi cập nhật';
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
  }

  deleteUser(user: User) {
    if (confirm(`Xóa người dùng "${user.fullName}"?`)) {
      // DELETE /api/users/:id
      this.userService.delete(user.userId).subscribe({
        next: () => {
          this.toastService.success('Đã xóa người dùng');
          this.loadUsers();
        },
        error: () => this.toastService.error('Lỗi xóa người dùng'),
      });
    }
  }

  getUserCountByRole(role: string) {
    return this.users().filter((u) => u.role === role).length;
  }
}
