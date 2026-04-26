  import { Component, OnInit, inject, signal } from '@angular/core';
  import { CommonModule } from '@angular/common';
  import { FormsModule } from '@angular/forms';
  import { TranslatePipe } from '../../../core/pipes/translate.pipe';
  import { AuthService } from '../../../core/services/auth.service';
  import { CloudinaryService } from '../../../core/services/cloudinary.service';
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
  cloudinaryService = inject(CloudinaryService);
  toastService = inject(ToastService);
  user = signal<UserDto | null>(null);

  // Form fields
  fullName = '';
  phone = '';
  
  // Avatar
  avatarPreview = signal<string | null>(null);  // Preview truoc khi upload
  isUploading = signal(false);   
  
  ngOnInit() {
    this.authService.getProfile().subscribe({
      next: (data) => {
        this.user.set(data);
        this.fullName = data.fullName;
        this.phone = data.phone || '';
        this.avatarPreview.set(data.avatarUrl || null);
      },
    });
  }

  /**
     * Khi user chon file anh tu may tinh:
     * 1. Validate o Frontend (type, size)
     * 2. Hien thi Preview ngay lap tuc
     * 3. Upload len Cloudinary qua Backend
     * 4. Cap nhat avatar_url trong DB
  */

  onAvatarSelected(event: Event){
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    //Validate
    const error = this.cloudinaryService.validateImageFile(file);
    if (error){
      this.toastService.warning(error);
      return;
    }

    //Preview ngay (khong can upload)
    const reader = new FileReader();
    reader.onload = () => {
      this.avatarPreview.set(reader.result as string);
    };
    reader.readAsDataURL(file);

    //Uplaod
    this.isUploading.set(true);
    this.cloudinaryService.uploadAvatar(file).subscribe({
      next: (res) => {
        this.isUploading.set(false);
        this.user.set(res.user);
        this.avatarPreview.set(res.imageUrl);
        // Cap nhat user trong localStorage de sidebar cung thay anh moi
        if (typeof localStorage !== 'undefined') {
          localStorage.setItem('user', JSON.stringify(res.user));
        }
        this.toastService.success("Cập nhật ảnh đại diện thành công!");
      },
      error: (err) => {
        this.isUploading.set(false);
        this.toastService.error(err.error?.message || 'Lỗi upload ảnh.');
      },
    })
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
