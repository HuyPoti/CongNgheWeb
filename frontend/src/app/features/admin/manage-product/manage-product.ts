import { Component, signal, inject, OnInit, PLATFORM_ID, ChangeDetectorRef } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../../core/services/product.service';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { CategoryService } from '../../../core/services/category.service';
import { BrandService } from '../../../core/services/brand.service';
import { ToastService } from '../../../core/services/toast.service';
import { CloudinaryService } from '../../../core/services/cloudinary.service';
import {
  ProductDto,
  ProductFullDto,
  CreateProductDto,
  UpdateProductDto,
} from '../../../core/models/product.model';
import { Category } from '../../../core/models/category.model';
import { Brand } from '../../../core/models/brand.model';
import { forkJoin, firstValueFrom } from 'rxjs';

interface ProductFormModel {
  categoryId: string;
  brandId: string;
  name: string;
  slug: string;
  sku: string;
  regularPrice: number;
  salePrice: number | null;
  stockQuantity: number;
  warrantyMonths: number;
  description: string;
  status: number;
  // UI-only fields for inline image/spec management
  imageUrls: { id?: string; url: string; isPrimary: boolean }[];
  specs: { key: string; value: string }[];
  newImageUrl: string;
  newSpecKey: string;
  newSpecValue: string;
}

@Component({
  selector: 'app-manage-product',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './manage-product.html',
})
export class ManageProduct implements OnInit {
  private productService = inject(ProductService);
  private categoryService = inject(CategoryService);
  private brandService = inject(BrandService);
  private toast = inject(ToastService);
  private cloudinary = inject(CloudinaryService);
  private platformId = inject(PLATFORM_ID);
  private cdr = inject(ChangeDetectorRef);

  products = signal<ProductDto[]>([]);
  categories = signal<Category[]>([]);
  brands = signal<Brand[]>([]);
  isLoading = signal(true);

  showModal = signal(false);
  isSaving = signal(false);
  editingProduct = signal<ProductDto | null>(null);
  editingFull = signal<ProductFullDto | null>(null);

  isUploadingFile = signal(false);
  isUploadingImage = signal(false);

  form: ProductFormModel = this.emptyForm();

  ngOnInit() {
    if (isPlatformBrowser(this.platformId)) {
      this.loadData();
    }
    console.log(this.products());
  }

  loadData() {
    this.isLoading.set(true);
    forkJoin({
      products: this.productService.getAll({ page: 1, pageSize: 50 }),
      categories: this.categoryService.getAll(),
      brands: this.brandService.getAll(),
    }).subscribe({
      next: ({ products, categories, brands }) => {
        this.products.set(products.items);
        this.categories.set(categories);
        this.brands.set(brands);
        this.isLoading.set(false);
      },
      error: () => {
        this.toast.error('Không thể tải dữ liệu');
        this.isLoading.set(false);
      },
    });
  }

  openCreate() {
    this.editingProduct.set(null);
    this.editingFull.set(null);
    this.form = this.emptyForm();
    this.showModal.set(true);
  }

  openEdit(product: ProductDto) {
    this.editingProduct.set(product);
    this.form = {
      categoryId: '', // will populate after loading full
      brandId: product.brandId,
      name: product.name,
      slug: product.slug,
      sku: product.sku ?? '',
      regularPrice: product.regularPrice,
      salePrice: product.salePrice,
      stockQuantity: product.stockQuantity,
      warrantyMonths: product.warrantyMonths,
      description: product.description ?? '',
      status: product.status, // Cast for local handling
      imageUrls: [],
      specs: [],
      newImageUrl: '',
      newSpecKey: '',
      newSpecValue: '',
    };
    // Load full product to get images
    this.productService.getFullById(product.productId).subscribe({
      next: (full) => {
        this.editingFull.set(full);
        this.form = {
          ...this.form,
          categoryId: this.getCategoryIdByName(product.categoryName) ?? '',
          imageUrls: full.images.map((img) => ({
            id: img.imageId,
            url: img.imageUrl,
            isPrimary: img.isPrimary,
          }))
        };
        
        // Parse specs from JSON specifications field
        if (product.specifications) {
          try {
            const parsed = JSON.parse(product.specifications);
            if (typeof parsed === 'object' && parsed !== null) {
              this.form.specs = Object.entries(parsed).map(([key, value]) => ({
                key,
                value: String(value)
              }));
            }
          } catch (e) {
            console.warn('Failed to parse specifications JSON in Admin', e);
          }
        }
        this.cdr.detectChanges();
      },
    });
    this.showModal.set(true);
  }

  closeModal() {
    this.showModal.set(false);
    this.editingProduct.set(null);
  }

  autoSlug() {
    if (this.form.name) {
      this.form.slug = this.form.name
        .toLowerCase()
        .normalize('NFD')
        .replace(/[\u0300-\u036f]/g, '')
        .replace(/\s+/g, '-')
        .replace(/[^a-z0-9-]/g, '');
    }
  }

  addImage() {
    if (this.form.newImageUrl.trim()) {
      const isPrimary = this.form.imageUrls.length === 0;
      this.form.imageUrls.push({ url: this.form.newImageUrl.trim(), isPrimary });
      this.form.newImageUrl = '';
    }
  }

  onImageFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) {
      const error = this.cloudinary.validateImageFile(file);
      if (error) {
        this.toast.error(error);
        return;
      }
      this.isUploadingImage.set(true);
      this.cloudinary.uploadImage('products', file).subscribe({
        next: (res) => {
          const isPrimary = this.form.imageUrls.length === 0;
          this.form.imageUrls.push({ url: res.imageUrl, isPrimary });
          this.isUploadingImage.set(false);
          // reset input file value if needed, handled by UI usually or we can ignore
          input.value = '';
          this.cdr.detectChanges();
        },
        error: () => {
          this.toast.error('Lỗi khi upload ảnh');
          this.isUploadingImage.set(false);
          this.cdr.detectChanges();
        }
      });
    }
  }

  removeImage(idx: number) {
    this.form.imageUrls.splice(idx, 1);
    // If removed image was primary, make first remaining one primary
    if (this.form.imageUrls.length > 0 && !this.form.imageUrls.some((i) => i.isPrimary)) {
      this.form.imageUrls[0].isPrimary = true;
    }
  }

  setPrimary(idx: number) {
    this.form.imageUrls.forEach((img, i) => (img.isPrimary = i === idx));
  }

  addSpec() {
    if (this.form.newSpecKey.trim() && this.form.newSpecValue.trim()) {
      this.form.specs.push({
        key: this.form.newSpecKey.trim(),
        value: this.form.newSpecValue.trim(),
      });
      this.form.newSpecKey = '';
      this.form.newSpecValue = '';
    }
  }

  removeSpec(idx: number) {
    this.form.specs.splice(idx, 1);
  }

  save() {
    if (!this.form.name?.trim()) {
      this.toast.warning('Tên sản phẩm không được để trống');
      return;
    }
    if (!this.form.categoryId) {
      this.toast.warning('Vui lòng chọn danh mục cho sản phẩm');
      return;
    }
    if (this.form.regularPrice <= 0) {
      this.toast.warning('Giá niêm yết phải lớn hơn 0');
      return;
    }
    if (this.form.salePrice !== null && this.form.salePrice < 0) {
      this.toast.warning('Giá khuyến mãi không được âm');
      return;
    }
    if (this.form.salePrice && this.form.salePrice >= this.form.regularPrice) {
      this.toast.warning('Giá khuyến mãi phải nhỏ hơn giá niêm yết');
      return;
    }
    if (this.form.stockQuantity < 0 || !Number.isInteger(this.form.stockQuantity)) {
      this.toast.warning('Số lượng tồn kho phải là số nguyên không âm');
      return;
    }
    if (this.form.warrantyMonths < 0 || !Number.isInteger(this.form.warrantyMonths)) {
      this.toast.warning('Thời gian bảo hành phải là số nguyên không âm');
      return;
    }
    if (this.form.sku?.trim() && !/^[a-zA-Z0-9-_]+$/.test(this.form.sku.trim())) {
      this.toast.warning('SKU chỉ được chứa ký tự chữ, số, dấu gạch ngang (-) hoặc gạch dưới (_)');
      return;
    }

    if (!this.form.imageUrls || this.form.imageUrls.length === 0) {
      this.toast.warning('Vui lòng thêm ít nhất một ảnh');
      return;
    }

    if (!this.form.brandId) {
      this.toast.warning('Vui lòng chọn thương hiệu cho sản phẩm');
      return;
    }

    this.isSaving.set(true);

    this.saveAsync().then(() => {
      this.isSaving.set(false);
    }).catch(err => {
      const msg = err?.error?.message || 'Có lỗi xảy ra';
      this.toast.error(msg);
      this.isSaving.set(false);
    });
  }

  private async saveAsync() {
    if (this.editingProduct()) {
      await this.updateProductAsync();
      this.toast.success('Cập nhật sản phẩm thành công!');
    } else {
      await this.createProductAsync();
      this.toast.success('Tạo sản phẩm thành công!');
    }
    this.loadData();
    this.closeModal();
  }

  private async createProductAsync() {
    const dto: CreateProductDto = {
      categoryId: this.form.categoryId,
      brandId: this.form.brandId,
      name: this.form.name,
      slug: this.form.slug,
      sku: this.form.sku || undefined,
      regularPrice: this.form.regularPrice,
      salePrice: this.form.salePrice,
      stockQuantity: this.form.stockQuantity,
      warrantyMonths: this.form.warrantyMonths,
      description: this.form.description || undefined,
      specifications: this.form.specs.length 
        ? JSON.stringify(this.form.specs.reduce((acc, s) => ({ ...acc, [s.key]: s.value }), {})) 
        : undefined,
      status: Number(this.form.status) || 1, 
    };

    const created = await firstValueFrom(this.productService.create(dto));

    // Add images
    for (let i = 0; i < this.form.imageUrls.length; i++) {
      const img = this.form.imageUrls[i];
      await firstValueFrom(
        this.productService.addImage(created.productId, {
          imageUrl: img.url,
          isPrimary: img.isPrimary,
          sortOrder: i,
        })
      );
    }
  }

  private async updateProductAsync() {
    const id = this.editingProduct()!.productId;
    const dto: UpdateProductDto = {
      categoryId: this.form.categoryId || undefined,
      brandId: this.form.brandId || undefined,
      name: this.form.name,
      slug: this.form.slug,
      sku: this.form.sku || undefined,
      regularPrice: this.form.regularPrice,
      salePrice: this.form.salePrice,
      stockQuantity: this.form.stockQuantity,
      warrantyMonths: this.form.warrantyMonths,
      description: this.form.description || undefined,
      specifications: this.form.specs.length 
        ? JSON.stringify(this.form.specs.reduce((acc, s) => ({ ...acc, [s.key]: s.value }), {})) 
        : undefined,
      status: Number(this.form.status),
    };

    await firstValueFrom(this.productService.update(id, dto));

    // Update images logic
    const originalImages = this.editingFull()?.images || [];
    const currentImages = this.form.imageUrls;

    // Find deleted images
    const deletedImages = originalImages.filter(orig => !currentImages.some(curr => curr.id === orig.imageId));
    
    // Find new images (no id)
    const newImages = currentImages.filter(curr => !curr.id);

    // Delete removed images
    for (const img of deletedImages) {
      try {
        await firstValueFrom(this.productService.deleteImage(id, img.imageId));
      } catch (e) {
        console.error('Failed to delete image', e);
      }
    }

    // Add new images
    for (let i = 0; i < newImages.length; i++) {
      const img = newImages[i];
      try {
        await firstValueFrom(
          this.productService.addImage(id, {
            imageUrl: img.url,
            isPrimary: img.isPrimary,
            sortOrder: originalImages.length + i,
          })
        );
      } catch (e) {
        console.error('Failed to add image', e);
      }
    }
  }

  // Deletion disabled as per user request

  toggleStatus(product: ProductDto) {
    const newStatus = Number(product.status) === 2 ? 1 : 2;
    this.productService.update(product.productId, { status: newStatus }).subscribe({
      next: () => {
        this.toast.success('Đã đổi trạng thái');
        this.loadData();
      },
      error: () => this.toast.error('Lỗi đổi trạng thái'),
    });
  }

  getBrandName(brandId: string): string {
    return this.brands().find(b => b.brandId === brandId)?.name || '—';
  }

  private getCategoryIdByName(name: string): string | undefined {
    return this.categories().find((c) => c.name === name)?.categoryId;
  }

  private emptyForm(): ProductFormModel {
    return {
      categoryId: '',
      brandId: '',
      name: '',
      slug: '',
      sku: '',
      regularPrice: 0,
      salePrice: null,
      stockQuantity: 0,
      warrantyMonths: 12,
      description: '',
      status: 1, // 1: Draft
      imageUrls: [],
      specs: [],
      newImageUrl: '',
      newSpecKey: '',
      newSpecValue: '',
    };
  }
}
