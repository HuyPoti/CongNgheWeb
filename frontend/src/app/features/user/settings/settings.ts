import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ThemeService } from '../../../core/services/theme';
import { LanguageService, Language } from '../../../core/services/language.service';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { ChangePasswordDto } from '../../../core/models/auth.models';

@Component({
  selector: 'app-settings',
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './settings.html',
})
export class Settings {
  themeService = inject(ThemeService);
  langService = inject(LanguageService);
  authService = inject(AuthService);
  toastService = inject(ToastService);

  activeTab = signal('preferences');
  securityView = signal<'overview' | 'change-password'>('overview');

  // Change Password Form
  currentPassword = '';
  newPassword = '';
  confirmPassword = '';

  tabs = [
    { id: 'preferences', label: 'settings.tab_preferences', icon: 'tune' },
    { id: 'security', label: 'settings.tab_security', icon: 'security' },
  ];

  updateTab(tabId: string) {
    this.activeTab.set(tabId);
    if (tabId === 'security') {
      this.securityView.set('overview');
      this.resetPasswordForm();
    }
  }

  setTheme(theme: 'light' | 'dark') {
    if (this.themeService.theme() !== theme) {
      this.themeService.toggleTheme();
    }
  }

  setLanguage(lang: Language) {
    this.langService.setLanguage(lang);
  }

  changePassword() {
    // 1. Frontend Validation
    if (!this.currentPassword || !this.newPassword || !this.confirmPassword) {
      this.toastService.warning('Vui lòng điền đầy đủ các trường mật khẩu!');
      return;
    }

    if (this.newPassword.length < 6) {
      this.toastService.warning('Mật khẩu mới phải có ít nhất 6 ký tự!');
      return;
    }

    if (this.newPassword !== this.confirmPassword) {
      this.toastService.warning('Mật khẩu xác nhận không khớp!');
      return;
    }

    const dto: ChangePasswordDto = {
      currentPassword: this.currentPassword,
      newPassword: this.newPassword,
    };

    this.authService.changePassword(dto).subscribe({
      next: (res) => {
        this.toastService.success(res.message);
        this.securityView.set('overview');
        this.resetPasswordForm();
      },
      error: (err) => {
        let errorMsg = 'Đã có lỗi xảy ra. Vui lòng thử lại.';

        // Trường hợp Backend trả về lỗi business logic { message: "..." }
        if (err.error?.message) {
          errorMsg = err.error.message;
        }
        // Trường hợp Backend trả về lỗi Validation { errors: { "Field": ["Error"] } }
        else if (err.error?.errors) {
          const firstErrorKey = Object.keys(err.error.errors)[0];
          errorMsg = err.error.errors[firstErrorKey][0];
        }

        this.toastService.error(errorMsg);
      },
    });
  }

  private resetPasswordForm() {
    this.currentPassword = '';
    this.newPassword = '';
    this.confirmPassword = '';
  }
}
