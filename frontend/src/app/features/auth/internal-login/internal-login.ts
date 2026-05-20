import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-internal-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './internal-login.html',
})
export class InternalLoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private toast = inject(ToastService);

  isLoading = signal(false);

  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', Validators.required],
  });

  onSubmit() {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);
    const { email, password } = this.loginForm.value;

    this.authService.login({ email: email!, password: password! }).subscribe({
      next: (res) => {
        const role = res.user.role.toLowerCase();
        
        // Kiểm tra quyền truy cập hệ thống nội bộ
        if (role === 'admin') {
          this.toast.success(`Welcome back Admin, ${res.user.fullName}!`);
          this.router.navigate(['/admin/dashboard']);
        } else if (role === 'staff') {
          this.toast.success(`Welcome back Staff, ${res.user.fullName}!`);
          this.router.navigate(['/employee/orders']);
        } else if (role === 'warehouse') {
          this.toast.success(`Welcome back Warehouse Staff, ${res.user.fullName}!`);
          this.router.navigate(['/employee/warehouse-orders']);
        } else {
          // Là khách hàng, chặn truy cập
          this.authService.logout();
          this.toast.error('Truy cập bị từ chối! Khu vực này chỉ dành cho nhân sự nội bộ.');
          this.isLoading.set(false);
        }
      },
      error: (err) => {
        this.toast.error(err.error?.message || 'Đăng nhập thất bại.');
        this.isLoading.set(false);
      },
      complete: () => {
        if (this.authService.currentUserValue?.role !== 'customer') {
          this.isLoading.set(false);
        }
      }
    });
  }
}
