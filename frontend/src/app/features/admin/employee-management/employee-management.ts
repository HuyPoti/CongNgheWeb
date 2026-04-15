import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../../core/services/user.service';
import { ToastService } from '../../../core/services/toast.service';
import { User, CreateUser, UpdateUser } from '../../../core/models/user.model';

@Component({
  selector: 'app-employee-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './employee-management.html',
  styles: ``,
})
export class EmployeeManagement implements OnInit {
  private userService = inject(UserService);
  private toastService = inject(ToastService);

  users = signal<User[]>([]);        
  isLoading = signal(true);          
  errorMessage = signal('');         

  showModal = signal(false);
  isCreating = signal(false);        
  editingUser = signal<User | null>(null);
  form: Partial<User & { password?: string }> = {};
  searchQuery = '';

  roleLabels: Record<string, string> = {
    customer: 'Khách hàng',
    admin: 'Quản trị viên',
    staff: 'Nhân viên',
  };

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
      error: (err) => {                     
        console.error('Lỗi tải nhân viên:', err);
        this.errorMessage.set('Không thể tải danh sách nhân viên');
        this.isLoading.set(false);
      },
    });
  }

  get filteredUsers() {
    const q = this.searchQuery.toLowerCase();
    const employees = this.users().filter(u => u.role === 'admin' || u.role === 'staff');
    
    if (!q) return employees;
    return employees.filter(
      (u) =>
        u.fullName.toLowerCase().includes(q) ||
        u.email.toLowerCase().includes(q) ||
        (u.phone && u.phone.includes(q))
    );
  }

  openCreate() {
    this.isCreating.set(true);
    this.editingUser.set(null);
    this.form = { role: 'staff' };
    this.showModal.set(true);
  }

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
    if (!this.form.fullName?.trim() || !this.form.email?.trim()) {
      this.toastService.warning('Vui lòng điền đầy đủ Họ tên và Email');
      return;
    }

    if (this.isCreating() && !this.form.password?.trim()) {
      this.toastService.warning('Vui lòng nhập mật khẩu cho tài khoản mới');
      return;
    }

    if (this.isCreating()) {
      const createData: CreateUser = {
        email: this.form.email || '',
        password: this.form.password || '',
        fullName: this.form.fullName || '',
        phone: this.form.phone || '', 
        role: this.form.role || 'staff',
      };
      this.userService.create(createData).subscribe({
        next: () => {
          this.toastService.success('Thêm nhân viên thành công');
          this.loadUsers();
          this.closeModal();
          this.errorMessage.set('');
        },
        error: (err) => {
          let msg = 'Lỗi tạo nhân viên';
          if (err.error?.message) {
            msg = err.error.message;
          }
          this.errorMessage.set(msg);
          this.toastService.error(msg);
        },
      });
    } else {
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
          if (err.error?.message) msg = err.error.message;
          this.errorMessage.set(msg);
          this.toastService.error(msg);
        },
      });
    }
  }

  deleteUser(user: User) {
    if (confirm(`Xóa nhân viên "${user.fullName}"?`)) {
      this.userService.delete(user.userId).subscribe({
        next: () => {
          this.toastService.success('Đã xóa nhân viên');
          this.loadUsers();
        },
        error: () => this.toastService.error('Lỗi xóa nhân viên'),
      });
    }
  }

  getEmployeeCount() {
    return this.users().filter((u) => u.role === 'admin' || u.role === 'staff').length;
  }
}
