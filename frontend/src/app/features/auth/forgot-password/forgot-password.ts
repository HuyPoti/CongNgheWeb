import { ChangeDetectorRef, Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-forgot-password',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule],
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.css',
})
export class ForgotPassword {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private toastService = inject(ToastService);
  private cdr = inject(ChangeDetectorRef);

  currentStep = 1;
  isLoading = false;
  email = '';

  emailForm: FormGroup;
  resetForm: FormGroup;

  constructor() {
    this.emailForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });

    this.resetForm = this.fb.group({
      otpCode: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  onSendOtp(): void {
    if (this.emailForm.invalid) {
      this.toastService.warning('Vui lòng nhập email hợp lệ.');
      return;
    }

    this.isLoading = true;
    this.cdr.detectChanges();
    this.email = this.emailForm.value.email;

    this.authService.forgotPassword({ email: this.email }).subscribe({
      next: (res) => {
        this.isLoading = false;
        this.currentStep = 2;
        this.cdr.detectChanges();
        this.toastService.success(res.message);
      },
      error: (err) => {
        this.isLoading = false;
        this.cdr.detectChanges();
        this.toastService.error(err.error?.message || 'Đã xảy ra lỗi.');
      },
    });
  }

  onResetPassword(): void {
    if (this.resetForm.invalid) {
      this.toastService.warning('Vui lòng điền đầy đủ thông tin.');
      return;
    }

    const { otpCode, newPassword, confirmPassword } = this.resetForm.value;

    if (newPassword !== confirmPassword) {
      this.toastService.warning('Mật khẩu xác nhận không khớp.');
      return;
    }

    this.isLoading = true;
    this.cdr.detectChanges();
    this.authService.resetPassword({ email: this.email, otpCode, newPassword }).subscribe({
      next: (res) => {
        this.isLoading = false;
        this.cdr.detectChanges();
        this.toastService.success(res.message);
        this.router.navigate(['/auth/login']); // Chuyển về trang đăng nhập
      },
      error: (err) => {
        this.isLoading = false;
        this.cdr.detectChanges();
        this.toastService.error(err.error?.message || 'Mã OTP không hợp lệ.');
      },
    });
  }

  goBackToStep1(): void {
    this.currentStep = 1;
    this.resetForm.reset();
  }
}
