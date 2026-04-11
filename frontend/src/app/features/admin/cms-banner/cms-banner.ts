import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Banner, CreateBanner, UpdateBanner } from '../../../core/models/banner.model';
import { BannerService } from '../../../core/services/banner.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-cms-banner',
  imports: [CommonModule, FormsModule],
  templateUrl: './cms-banner.html',
  styles: ``,
})
export class CmsBanner implements OnInit {
  private bannerService = inject(BannerService);
  private toast = inject(ToastService);

  banners = signal<Banner[]>([]);
  isLoading = signal(false);

  showModal = signal(false);
  editingBanner = signal<Banner | null>(null);

  positionLabels: Record<string, string> = {
    homepage_slider: 'Homepage Slider',
    homepage_mid: 'Homepage Mid',
    category_top: 'Category Top',
    news_top: 'News Top'
  };

  form: Partial<Banner> = {};

  ngOnInit(): void {
    this.loadBanners();
  }

  loadBanners() {
    this.isLoading.set(true);
    this.bannerService.getAll().subscribe({
      next: banners => {
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
    return this.banners().filter(b => b.isActive).length;
  }

  openCreateModal() {
    this.editingBanner.set(null);
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
    this.editingBanner.set(banner);
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
    if (!this.form.imageUrl?.trim()) {
      this.toast.warning('Vui lòng nhập URL hình ảnh');
      return;
    }

    const payload = this.buildPayload();

    if (this.editingBanner()) {
      this.bannerService.update(this.editingBanner()!.bannerId, payload).subscribe({
        next: () => {
          this.toast.success('Cập nhật banner thành công');
          this.loadBanners();
          this.closeModal();
        },
        error: () => this.toast.error('Lỗi khi cập nhật banner')
      });
    } else {
      this.bannerService.create(payload).subscribe({
        next: () => {
          this.toast.success('Thêm banner mới thành công');
          this.loadBanners();
          this.closeModal();
        },
        error: () => this.toast.error('Lỗi khi tạo banner')
      });
    }
  }

  toggleStatus(banner: Banner) {
    const newStatus = !banner.isActive;
    
    this.bannerService.update(banner.bannerId, { isActive: newStatus }).subscribe({
      next: (updated) => {
        this.banners.update((list) =>
          list.map((b) => (b.bannerId === banner.bannerId ? { ...b, isActive: updated.isActive } : b))
        );
        this.toast.success(`Đã ${newStatus ? 'kích hoạt' : 'tạm ngưng'} banner thành công`);
      },
      error: () => this.toast.error('Lỗi khi đổi trạng thái banner')
    });
  }

  deleteBanner(banner: Banner) {
    // Feature removed as requested. We keep the method empty if needed for other parts, or just remove it.
    // Since there are no references left in HTML, we can safely remove it.
  }

  private buildPayload(): CreateBanner {
    return {
      title: this.form.title?.trim() || null,
      subtitle: this.form.subtitle?.trim() || null,
      imageUrl: this.form.imageUrl?.trim() || '',
      linkUrl: this.form.linkUrl?.trim() || null,
      position: this.form.position as any || 'homepage_slider',
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

