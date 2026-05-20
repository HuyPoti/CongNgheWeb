import { Component, signal, computed, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CategoryService } from '../../../core/services/category.service';
import { ToastService } from '../../../core/services/toast.service';
import { ConfirmService } from '../../../core/services/confirm.service';
import { CloudinaryService } from '../../../core/services/cloudinary.service';
import { Category, CreateCategory, UpdateCategory } from '../../../core/models/category.model';

@Component({
  selector: 'app-category-hierarchy',
  imports: [CommonModule, FormsModule],
  templateUrl: './category-hierarchy.html',
  styles: ``,
})
export class CategoryHierarchy implements OnInit {
  private categoryService = inject(CategoryService);
  private toastService = inject(ToastService);
  private confirmService = inject(ConfirmService);
  private cloudinary = inject(CloudinaryService);

  categories = signal<Category[]>([]);
  isLoading = signal(true);

  showModal = signal(false);
  isCreating = signal(false);
  editingCategory = signal<Category | null>(null);
  form: Partial<Category> = {};

  isUploadingFile = signal(false);
  selectedImageFile = signal<File | null>(null);
  isSubmitting = signal(false);

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) {
      const error = this.cloudinary.validateImageFile(file);
      if (error) {
        this.toastService.error(error);
        return;
      }
      this.selectedImageFile.set(file);
      this.form.imageUrl = URL.createObjectURL(file);
    }
  }

  rootCategories = computed(() => this.categories().filter(c => !c.parentId));

  ngOnInit() {
    this.loadCategories();
  }

  loadCategories() {
    this.isLoading.set(true);
    this.categoryService.getAll().subscribe({
      next: (data) => {
        this.categories.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Lỗi tải danh mục:', err);
        this.toastService.error('Không thể tải danh sách danh mục sản phẩm!');
        this.isLoading.set(false);
      },
    });
  }

  getChildren(parentId: string) {
    return this.categories().filter(c => c.parentId === parentId);
  }

  getParentName(parentId: string | null): string {
    if (!parentId) return 'Gốc (Root)';
    return this.categories().find(c => c.categoryId === parentId)?.name || 'N/A';
  }

  openCreate(parentId: string | null = null) {
    this.editingCategory.set(null);
    this.isUploadingFile.set(false);
    this.selectedImageFile.set(null);
    this.form = { name: '', slug: '', description: '', parentId: parentId, imageUrl: '', isActive: true };
    this.showModal.set(true);
  }

  openEdit(cat: Category) {
    this.editingCategory.set(cat);
    this.isUploadingFile.set(false);
    this.selectedImageFile.set(null);
    this.form = { ...cat };
    this.showModal.set(true);
  }

  closeModal() { this.showModal.set(false); }

  autoSlug() {
    if (this.form.name) {
      this.form.slug = this.form.name.toLowerCase().replace(/\s+/g, '-').replace(/[^a-z0-9-]/g, '');
    }
  }

  save() {
    // === VALIDATION TRƯỚC KHI GỬI ===
    if (!this.form.name?.trim() || !this.form.slug?.trim()) {
      this.toastService.warning('Vui lòng điền đầy đủ Tên và Slug danh mục');
      return; 
    }

    if (this.isUploadingFile() && this.selectedImageFile()) {
      this.isSubmitting.set(true);
      this.cloudinary.uploadImage('categories', this.selectedImageFile()!).subscribe({
        next: (res) => {
          this.form.imageUrl = res.imageUrl;
          this.submitData();
        },
        error: () => {
          this.toastService.error('Lỗi khi upload ảnh');
          this.isSubmitting.set(false);
        }
      });
    } else {
      this.submitData();
    }
  }

  private submitData() {
    this.isSubmitting.set(true);

    if (this.editingCategory()) {
      // CẬP NHẬT (PUT)
      const categoryId = this.editingCategory()!.categoryId;
      const updateData: UpdateCategory = {
        name: this.form.name,
        slug: this.form.slug,
        description: this.form.description,
        parentId: this.form.parentId,
        imageUrl: this.form.imageUrl,
        isActive: this.form.isActive,
      };

      this.categoryService.update(categoryId, updateData).subscribe({
        next: () => {
          this.toastService.success('Cập nhật danh mục thành công!');
          this.loadCategories();
          this.closeModal();
          this.isSubmitting.set(false);
        },
        error: (err) => {
          let msg = 'Lỗi khi cập nhật danh mục';
          if (err.error?.message) msg = err.error.message;
          else if (err.error?.errors) {
            const keys = Object.keys(err.error.errors);
            if (keys.length > 0) msg = err.error.errors[keys[0]][0];
          }
          this.toastService.error(msg);
          this.isSubmitting.set(false);
        }
      });
    } else {
      // TẠO MỚI (POST)
      const createData: CreateCategory = {
        name: this.form.name || '',
        slug: this.form.slug || '',
        description: this.form.description || '',
        parentId: this.form.parentId ?? null,
        imageUrl: this.form.imageUrl || '',
        isActive: this.form.isActive ?? true,
      };
      this.categoryService.create(createData).subscribe({
        next: () => {
          this.toastService.success('Danh mục đã được tạo thành công!');
          this.loadCategories();
          this.closeModal();
          this.isSubmitting.set(false);
        },
        error: (err) => {
          let msg = 'Lỗi khi tạo danh mục';
          if (err.error?.message) msg = err.error.message;
          else if (err.error?.errors) {
            const keys = Object.keys(err.error.errors);
            if (keys.length > 0) msg = err.error.errors[keys[0]][0];
          }
          this.toastService.error(msg);
          this.isSubmitting.set(false);
        }
      }); 
    }
  }

  toggleActive(cat: Category) {
    const updateData: UpdateCategory = { isActive: !cat.isActive };
    this.categoryService.update(cat.categoryId, updateData).subscribe({
      next: () => {
        this.toastService.success('Đã cập nhật trạng thái');
        this.loadCategories();
      },
      error: () => this.toastService.error('Lỗi đổi trạng thái')
    });
  }

  async deleteCategory(cat: Category) {
    const isConfirmed = await this.confirmService.confirm(
      `Bạn có chắc chắn muốn xóa danh mục "${cat.name}"? Các danh mục con cũng sẽ mất liên kết.`,
      'Xóa danh mục',
      'danger'
    );
    if (isConfirmed) {
      this.categoryService.delete(cat.categoryId).subscribe({
        next: () => {
          this.toastService.success('Đã xóa danh mục');
          this.loadCategories();
        },
        error: () => this.toastService.error('Lỗi xóa danh mục')
      });
    }
  }
}
