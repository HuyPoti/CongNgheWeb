import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BrandService } from '../../../core/services/brand.service';
import { ToastService } from '../../../core/services/toast.service';
import { Brand, CreateBrand, UpdateBrand } from '../../../core/models/brand.model';

@Component({
  selector: 'app-brand-management',
  imports: [CommonModule, FormsModule],
  templateUrl: './brand-management.html',
  styles: ``,
})
export class BrandManagement implements OnInit {
  private brandService = inject(BrandService);
  private toastService = inject(ToastService);

  brands = signal<Brand[]>([]);
  isLoading = signal(true);

  showModal = signal(false);
  isCreating = signal(false);
  editingBrand = signal<Brand | null>(null);
  form: Partial<Brand> = {};

  ngOnInit() {
    this.loadBrands();
  }

  loadBrands() {
    this.isLoading.set(true);
    this.brandService.getAll().subscribe({
      next: (data) => {
        this.brands.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.toastService.error('Không thể tải danh sách thương hiệu');
        this.isLoading.set(false);
      },
    });
  }

  openCreate() {
    this.editingBrand.set(null);
    this.form = { name: '', slug: '', logoUrl: '', description: '' };
    this.isCreating.set(true);
    this.showModal.set(true);
  }

  openEdit(brand: Brand) {
    this.editingBrand.set(brand);
    this.form = { ...brand };
    this.isCreating.set(false);
    this.showModal.set(true);
  }

  closeModal() {
    this.showModal.set(false);
  }

  autoSlug() {
    if (this.form.name) {
      this.form.slug = this.form.name
        .toLowerCase()
        .replace(/\s+/g, '-')
        .replace(/[^a-z0-9-]/g, '');
    }
  }

  save() {
    if (!this.form.name?.trim() || !this.form.slug?.trim()) {
      this.toastService.warning('Vui lòng điền đầy đủ Tên và Slug');
      return;
    }

    if (this.editingBrand()) {
      const brandId = this.editingBrand()!.brandId;
      const updateData: UpdateBrand = {
        name: this.form.name,
        slug: this.form.slug,
        logoUrl: this.form.logoUrl || null,
        description: this.form.description || null,
        isActive: this.form.isActive,
      };

      this.brandService.update(brandId, updateData).subscribe({
        next: (updated) => {
          this.brands.update((list) => list.map((b) => (b.brandId === brandId ? updated : b)));
          this.closeModal();
          this.toastService.success('Cập nhật thương hiệu thành công!');
        },
        error: (err) => {
          let msg = 'Lỗi khi cập nhật thương hiệu';
          if (err.status === 409) msg = 'Slug đã tồn tại';
          this.toastService.error(msg);
        },
      });
    } else {
      const createData: CreateBrand = {
        name: this.form.name || '',
        slug: this.form.slug || '',
        logoUrl: this.form.logoUrl || '',
        description: this.form.description || '',
      };

      this.brandService.create(createData).subscribe({
        next: (created) => {
          this.brands.update((list) => [...list, created]);
          this.closeModal();
          this.toastService.success('Thêm thương hiệu thành công!');
        },
        error: (err) => {
          let msg = 'Lỗi khi tạo thương hiệu';
          if (err.status === 409) msg = 'Slug đã tồn tại';
          this.toastService.error(msg);
        },
      });
    }
  }

  toggleStatus(brand: Brand) {
    const newStatus = !brand.isActive;
    this.brandService.update(brand.brandId, { isActive: newStatus }).subscribe({
      next: (updated) => {
        // Cập nhật signal bằng cách tạo mảng mới để trigger UI refresh ngay lập tức
        this.brands.update((list) =>
          list.map((b) => (b.brandId === brand.brandId ? { ...b, isActive: updated.isActive } : b)),
        );
        this.toastService.success(`Đã ${newStatus ? 'kích hoạt' : 'vô hiệu hóa'} thương hiệu`);
      },
      error: () => this.toastService.error('Không thể cập nhật trạng thái'),
    });
  }
}
