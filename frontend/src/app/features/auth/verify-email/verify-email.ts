import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
    selector: 'app-verify-email',
    standalone: true,
    imports: [CommonModule, FormsModule],
    template: `
        <div class="verify-container">
            <h2>Xác nhận Email</h2>
            <p>Nhập mã OTP đã được gửi đến <strong>{{ email }}</strong></p>

            <input type="text" [(ngModel)]="otpCode" maxlength="6"
                    placeholder="Nhập mã 6 số" class="otp-input" />

            <button (click)="verify()" [disabled]="loading" class="btn-primary">
                {{ loading ? 'Đang xác nhận...' : 'Xác nhận' }}
            </button>

            <button (click)="resend()" [disabled]="resendLoading" class="btn-secondary">
                {{ resendLoading ? 'Đang gửi...' : 'Gửi lại mã' }}
            </button>

            @if (message) {
                <p [class]="isError ? 'error' : 'success'">{{ message }}</p>
            }
        </div>
  `
})

export class VerifyEmail{
    private authService = inject(AuthService);
    private router = inject(Router);
    private route = inject(ActivatedRoute);

    email = '';
    otpCode = '';
    message = '';
    isError = false;
    loading = false;
    resendLoading = false;

    ngOnInit() {
        // Lấy email từ query params: /auth/verify-email?email=abc@gmail.com
        this.route.queryParams.subscribe(params => {
        this.email = params['email'] || '';
        });
    }

    verify(){
        this.loading = true;
        this.authService.verifyEmail({ email: this.email, otpCode: this.otpCode}).subscribe({
            next: (res) => {
                this.message = res.message;
                this.isError = false;
                this.loading = false;
                setTimeout(() => this.router.navigate(['/login']), 2000);
            },
            error: (err) => {
                this.message = err.error.message || 'Xác nhận thất bại';
                this.isError = true;
                this.loading = false;
            }
        })
    }

    resend() {
        this.resendLoading = true;
        this.authService.resendVerification({ email: this.email }).subscribe({
        next: (res) => {
            this.message = res.message;
            this.isError = false;
            this.resendLoading = false;
        },
        error: (err) => {
            this.message = err.error.message || 'Gửi lại thất bại';
            this.isError = true;
            this.resendLoading = false;
        }
    });
  }
}