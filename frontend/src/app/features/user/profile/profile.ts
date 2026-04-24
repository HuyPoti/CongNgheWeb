import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { UserDto, UpdateProfileDto } from '../../../core/models/auth.models';

@Component({
  selector: 'app-profile',
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile implements OnInit {
  authService = inject(AuthService);
  toastService = inject(ToastService);
  user = signal<UserDto | null>(null);

  // Form fields
  fullName = '';
  phone = '';

  ngOnInit() {
    this.authService.getProfile().subscribe({
      next: (data) => {
        this.user.set(data);
        this.fullName = data.fullName;
        this.phone = data.phone || '';
      },
    });
  }

  saveChanges() {
    const dto: UpdateProfileDto = {
      fullName: this.fullName,
      phone: this.phone,
    };

    if (!this.fullName || this.fullName.trim().length === 0) {
      this.toastService.warning('Họ và tên không được để trống!');
      return;
    }

    this.authService.updateProfile(dto).subscribe({
      next: (updatedUser) => {
        this.user.set(updatedUser);
        this.toastService.success('Cập nhật thông tin cá nhân thành công!');
      },
      error: (err) => {
        let errorMsg = 'Không thể cập nhật thông tin.';

        if (err.error?.message) {
          errorMsg = err.error.message;
        } else if (err.error?.errors) {
          const firstErrorKey = Object.keys(err.error.errors)[0];
          errorMsg = err.error.errors[firstErrorKey][0];
        }

        this.toastService.error(errorMsg);
      },
    });
  }
}
