import { Component, inject, OnInit, signal, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-verify-email',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './verify-email.html'
})
export class VerifyEmail implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private toastService = inject(ToastService);

  email = '';
  otpCode = '';
  loading = false;
  resendLoading = false;
  
  // Logic đếm ngược cho nút gửi lại mã
  countdown = signal(0);
  private timer: any;

  ngOnInit() {
    // Lấy email từ query params: /auth/verify-email?email=abc@gmail.com
    this.route.queryParams.subscribe(params => {
      this.email = params['email'] || '';
      if (!this.email) {
        this.toastService.warning('Thiếu thông tin email để xác thực.');
      }
    });

    // Tự động kích hoạt đếm ngược 60s khi vào trang
    this.startCountdown();
  }

  ngOnDestroy() {
    if (this.timer) clearInterval(this.timer);
  }

  startCountdown() {
    this.countdown.set(60);
    if (this.timer) clearInterval(this.timer);
    this.timer = setInterval(() => {
      this.countdown.update(val => val > 0 ? val - 1 : 0);
      if (this.countdown() === 0) clearInterval(this.timer);
    }, 1000);
  }

  verify() {
    if (this.otpCode.length < 6) return;

    this.loading = true;
    this.authService.verifyEmail({ email: this.email, otpCode: this.otpCode }).subscribe({
      next: (res) => {
        this.loading = false;
        this.toastService.success('Xác thực email thành công! Đang chuyển hướng...');
        setTimeout(() => this.router.navigate(['/auth/login']), 1500);
      },
      error: (err) => {
        this.loading = false;
        this.toastService.error(err.error?.message || 'Mã OTP không chính xác hoặc đã hết hạn.');
      }
    });
  }

  resend() {
    if (this.countdown() > 0) return;

    this.resendLoading = true;
    this.authService.resendVerification({ email: this.email }).subscribe({
      next: (res) => {
        this.resendLoading = false;
        this.toastService.success('Mã OTP mới đã được gửi vào email của bạn.');
        this.startCountdown(); // Reset lại bộ đếm
      },
      error: (err) => {
        this.resendLoading = false;
        this.toastService.error(err.error?.message || 'Gửi lại mã thất bại. Vui lòng thử lại sau.');
      }
    });
  }
}