import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { SocialAuthService, GoogleSigninButtonModule } from '@abacritt/angularx-social-login';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, GoogleSigninButtonModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private socialAuthService = inject(SocialAuthService);
  private toastService = inject(ToastService);

  loginForm: FormGroup;
  returnUrl = '/';
  isLoading = false;

  constructor() {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
    });
  }

  ngOnInit(): void {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';

    this.socialAuthService.authState.subscribe((socialUser) => {
      if (socialUser && socialUser.idToken) {
        this.isLoading = true;
        this.authService.googleLogin(socialUser.idToken).subscribe({
          next: () => {
            this.toastService.success('Đăng nhập thành công với Google');
            this.router.navigateByUrl(this.returnUrl);
          },
          error: (err) => {
            this.isLoading = false;
            const msg = err.error?.message || 'Lỗi đăng nhập Google';
            this.toastService.error(msg);
          },
        });
      }
    });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.toastService.warning('Vui lòng điền đầy đủ và đúng định dạng');
      return;
    }

    this.isLoading = true;
    this.authService.login(this.loginForm.value).subscribe({
      next: () => {
        this.toastService.success('Khởi tạo quyền truy cập thành công!');
        this.router.navigateByUrl(this.returnUrl);
      },
      error: (err) => {
        this.isLoading = false;
        const msg = err.error?.message || 'Email hoặc mật khẩu không chính xác';
        this.toastService.error(msg);
      },
    });
  }
}
