import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { SocialAuthService, GoogleSigninButtonModule } from '@abacritt/angularx-social-login';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, GoogleSigninButtonModule],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private socialAuthService = inject(SocialAuthService);
  private toastService = inject(ToastService);

  registerForm: FormGroup;
  returnUrl = '/';
  isLoading = false;

  constructor() {
    this.registerForm = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
    });

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
            this.toastService.error(err.error?.message || 'Lỗi kết nối Google');
          },
        });
      }
    });
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.toastService.warning('Vui lòng điền đúng và đủ thông tin');
      return;
    }

    this.isLoading = true;
    this.authService.register(this.registerForm.value).subscribe({
      next: () => {
        this.toastService.success('Tạo tài khoản thành công!');
        this.router.navigateByUrl(this.returnUrl);
      },
      error: (err) => {
        this.isLoading = false;
        this.toastService.error(err.error?.message || 'Lỗi tạo tài khoản');
      },
    });
  }
}
