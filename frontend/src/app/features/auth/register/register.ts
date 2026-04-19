import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { SocialAuthService, GoogleSigninButtonModule } from '@abacritt/angularx-social-login';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, RouterLink, ReactiveFormsModule, GoogleSigninButtonModule, TranslatePipe],
  templateUrl: './register.html',
  styleUrl: './register.css',
})
export class Register implements OnInit {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private socialAuthService = inject(SocialAuthService);
  private toastService = inject(ToastService);

  registerForm: FormGroup;
  returnUrl = '/';
  isLoading = false;
  submitted = false;

  constructor() {
    this.registerForm = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      terms: [false, Validators.requiredTrue],
    });

    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
  }

  ngOnInit(): void {
    this.socialAuthService.authState.subscribe((socialUser) => {
      if (socialUser && socialUser.idToken) {
        setTimeout(() => {
          this.isLoading = true;
          this.authService.googleLogin(socialUser.idToken as string).subscribe({
            next: () => {
              this.isLoading = false;
              this.toastService.success('Đăng nhập thành công với Google');
              this.router.navigateByUrl(this.returnUrl || '/');
            },
            error: (err) => {
              this.isLoading = false;
              this.toastService.error(err.error?.message || 'Lỗi kết nối Google');
            },
          });
        });
      }
    });
  }

  onSubmit(): void {
    this.submitted = true;
    if (this.registerForm.invalid) {
      const controls = this.registerForm.controls;

      if (controls['fullName'].invalid) {
        this.toastService.warning('Vui lòng nhập họ và tên');
        return;
      }

      if (controls['email'].invalid) {
        const emailErrors = controls['email'].errors;
        if (emailErrors?.['required']) {
          this.toastService.warning('Vui lòng nhập địa chỉ email');
        } else {
          this.toastService.warning('Định dạng email không hợp lệ');
        }
        return;
      }

      if (controls['password'].invalid) {
        const passErrors = controls['password'].errors;
        if (passErrors?.['required']) {
          this.toastService.warning('Vui lòng nhập mật khẩu');
        } else if (passErrors?.['minlength']) {
          this.toastService.warning('Mật khẩu phải có ít nhất 6 ký tự');
        }
        return;
      }

      if (controls['terms'].invalid) {
        this.toastService.warning('Vui lòng đồng ý với Điều khoản và Điều kiện để tiếp tục');
        return;
      }

      this.toastService.warning('Vui lòng điền đúng và đủ thông tin');
      return;
    }

    this.isLoading = true;
    const registerData = { ...this.registerForm.value };
    delete registerData.terms;

    this.authService.register(registerData).subscribe({
      next: () => {
        this.isLoading = false;
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
