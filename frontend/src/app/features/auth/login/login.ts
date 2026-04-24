import { Component, inject, OnInit, ChangeDetectorRef, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { SocialAuthService, GoogleSigninButtonModule } from '@abacritt/angularx-social-login';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, GoogleSigninButtonModule, TranslatePipe],
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
  private cdr = inject(ChangeDetectorRef);

  loginForm: FormGroup;
  returnUrl = '/';
  isLoading = signal(false);

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
        this.isLoading.set(true);
        this.authService.googleLogin(socialUser.idToken as string).subscribe({
          next: () => {
            this.isLoading.set(false);
            this.toastService.success('Đăng nhập thành công với Google');
            this.router.navigateByUrl(this.returnUrl || '/');
          },
          error: (err) => {
            this.isLoading.set(false);
            const msg = err.error?.message || 'Lỗi xác thực Google';
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

    this.isLoading.set(true);
    this.authService.login(this.loginForm.value).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.toastService.success('Đăng nhập thành công!');
        this.router.navigateByUrl(this.returnUrl);
      },
      error: (err) => {
        this.isLoading.set(false);
        console.log(err);
        const msg = err.error?.message || 'Đã có lỗi xảy ra trong quá trình đăng nhập';
        this.toastService.error(msg);
      },
    });
  }
}
