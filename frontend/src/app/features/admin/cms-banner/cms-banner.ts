import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Banner, CreateBanner } from '../../../core/models/banner.model';
import { BannerService } from '../../../core/services/banner.service';
import { ToastService } from '../../../core/services/toast.service';
import { CloudinaryService } from '../../../core/services/cloudinary.service';

@Component({
  selector: 'app-cms-banner',
  imports: [CommonModule, FormsModule],
  templateUrl: './cms-banner.html',
  styles: ``,
})
export class CmsBanner implements OnInit {
  private bannerService = inject(BannerService);
  private toast = inject(ToastService);
  private cloudinary = inject(CloudinaryService);

  banners = signal<Banner[]>([]);
  isLoading = signal(false);

  showModal = signal(false);
  editingBanner = signal<Banner | null>(null);
  
  isUploadingFile = signal(false);
  selectedImageFile = signal<File | null>(null);

  positionLabels: Record<string, string> = {
    homepage_slider: 'Slider chính (Trang chủ)',
    homepage_mid_top_right: 'Quảng cáo phụ 1 (Trên cùng bên phải)',
    homepage_mid_bottom_right: 'Quảng cáo phụ 2 (Dưới cùng bên phải)',
    homepage_mid_wide: 'Banner ngang lớn (Cuối trang chủ)',
  };

  showPositionMap = signal(false);

  form: Partial<Banner> = {};
  bannerImageError = signal<Record<string, boolean>>({});

  handleImageError(id: string) {
    this.bannerImageError.update((prev) => ({ ...prev, [id]: true }));
  }

  resetPreviewError() {
    this.bannerImageError.update((prev) => {
      const newState = { ...prev };
      delete newState['preview'];
      return newState;
    });
  }

  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      const error = this.cloudinary.validateImageFile(file);
      if (error) {
        this.toast.error(error);
        return;
      }
      this.selectedImageFile.set(file);
      this.form.imageUrl = URL.createObjectURL(file);
      this.resetPreviewError();
    }
  }

  ngOnInit(): void {
    this.loadBanners();
  }

  loadBanners() {
    this.isLoading.set(true);
    this.bannerService.getAll().subscribe({
      next: (banners) => {
        this.banners.set(banners);
        this.isLoading.set(false);
      },
      error: () => {
        this.banners.set([]);
        this.isLoading.set(false);
        this.toast.error('Lỗi khi tải danh sách banner');
      },
    });
  }

  get activeBannersCount() {
    return this.banners().filter((b) => b.isActive).length;
  }

  openCreateModal() {
    this.resetPreviewError();
    this.editingBanner.set(null);
    this.isUploadingFile.set(false);
    this.selectedImageFile.set(null);
    this.form = {
      title: '',
      subtitle: '',
      imageUrl: '',
      linkUrl: '',
      position: 'homepage_slider',
      sortOrder: 0,
      isActive: true,
      startDate: null,
      endDate: null,
    };
    this.showModal.set(true);
  }

  openEditModal(banner: Banner) {
    this.resetPreviewError();
    this.editingBanner.set(banner);
    this.isUploadingFile.set(false);
    this.selectedImageFile.set(null);
    this.form = {
      ...banner,
      startDate: this.toDateInputValue(banner.startDate),
      endDate: this.toDateInputValue(banner.endDate),
    };
    this.showModal.set(true);
  }

  closeModal() {
    this.showModal.set(false);
    this.editingBanner.set(null);
  }

  saveForm() {
    if (!this.form.imageUrl?.trim() && !this.selectedImageFile()) {
      this.toast.warning('Vui lòng nhập URL hình ảnh hoặc chọn file');
      return;
    }

    if (this.isUploadingFile() && this.selectedImageFile()) {
      this.isLoading.set(true);
      this.cloudinary.uploadImage('banners', this.selectedImageFile()!).subscribe({
        next: (res) => {
          this.form.imageUrl = res.imageUrl;
          this.submitData();
        },
        error: () => {
          this.toast.error('Lỗi khi upload ảnh');
          this.isLoading.set(false);
        }
      });
    } else {
      this.submitData();
    }
  }

  private submitData() {
    const payload = this.buildPayload();
    this.isLoading.set(true);

    if (this.editingBanner()) {
      this.bannerService.update(this.editingBanner()!.bannerId, payload).subscribe({
        next: () => {
          this.toast.success('Cập nhật banner thành công');
          this.loadBanners();
          this.closeModal();
        },
        error: () => {
          this.toast.error('Lỗi khi cập nhật banner');
          this.isLoading.set(false);
        },
      });
    } else {
      this.bannerService.create(payload).subscribe({
        next: () => {
          this.toast.success('Thêm banner mới thành công');
          this.loadBanners();
          this.closeModal();
        },
        error: () => {
          this.toast.error('Lỗi khi tạo banner');
          this.isLoading.set(false);
        },
      });
    }
  }

  toggleStatus(banner: Banner) {
    const newStatus = !banner.isActive;

    this.bannerService.update(banner.bannerId, { isActive: newStatus }).subscribe({
      next: (updated) => {
        this.banners.update((list) =>
          list.map((b) =>
            b.bannerId === banner.bannerId ? { ...b, isActive: updated.isActive } : b,
          ),
        );
        this.toast.success(`Đã ${newStatus ? 'kích hoạt' : 'tạm ngưng'} banner thành công`);
      },
      error: () => this.toast.error('Lỗi khi đổi trạng thái banner'),
    });
  }

  private buildPayload(): CreateBanner {
    return {
      title: this.form.title?.trim() || null,
      subtitle: this.form.subtitle?.trim() || null,
      imageUrl: this.form.imageUrl?.trim() || '',
      linkUrl: this.form.linkUrl?.trim() || null,
      position: this.form.position || 'homepage_slider',
      sortOrder: Number(this.form.sortOrder ?? 0),
      isActive: this.form.isActive ?? true,
      startDate: this.form.startDate || null,
      endDate: this.form.endDate || null,
    };
  }

  private toDateInputValue(value: string | null | undefined): string | null {
    if (!value) return null;
    return value.split('T')[0]; // Extract YYYY-MM-DD
  }
}
