import { Component, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService } from '../../../core/services/product.service';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { CategoryService } from '../../../core/services/category.service';
import { BrandService } from '../../../core/services/brand.service';
import { ToastService } from '../../../core/services/toast.service';
import {
  ProductDto,
  ProductFullDto,
  CreateProductDto,
  UpdateProductDto,
} from '../../../core/models/product.model';
import { Category } from '../../../core/models/category.model';
import { Brand } from '../../../core/models/brand.model';
import { forkJoin, of } from 'rxjs';
import { switchMap } from 'rxjs/operators';

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
  imageUrls: { url: string; isPrimary: boolean }[];
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

  products = signal<ProductDto[]>([]);
  categories = signal<Category[]>([]);
  brands = signal<Brand[]>([]);
  isLoading = signal(true);

  showModal = signal(false);
  isSaving = signal(false);
  editingProduct = signal<ProductDto | null>(null);
  editingFull = signal<ProductFullDto | null>(null);

  form: ProductFormModel = this.emptyForm();

  ngOnInit() {
    this.loadData();
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
        this.form.categoryId = this.getCategoryIdByName(product.categoryName) ?? '';
        this.form.imageUrls = full.images.map((img) => ({
          url: img.imageUrl,
          isPrimary: img.isPrimary,
        }));
        
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
    if (this.form.salePrice && this.form.salePrice >= this.form.regularPrice) {
      this.toast.warning('Giá khuyến mãi phải nhỏ hơn giá niêm yết');
      return;
    }

    this.isSaving.set(true);

    if (this.editingProduct()) {
      this.updateProduct();
    } else {
      this.createProduct();
    }
  }

  private createProduct() {
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

    this.productService
      .create(dto)
      .pipe(
        switchMap((created) => {
          const imageObs = this.form.imageUrls.map((img, i) =>
            this.productService.addImage(created.productId, {
              imageUrl: img.url,
              isPrimary: img.isPrimary,
              sortOrder: i,
            }),
          );
          return imageObs.length ? forkJoin(imageObs) : of([]);
        }),
      )
      .subscribe({
        next: () => {
          this.toast.success('Tạo sản phẩm thành công!');
          this.loadData();
          this.closeModal();
          this.isSaving.set(false);
        },
        error: (err) => {
          const msg = err?.error?.message || 'Lỗi tạo sản phẩm';
          this.toast.error(msg);
          this.isSaving.set(false);
        },
      });
  }

  private updateProduct() {
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

    this.productService.update(id, dto).subscribe({
      next: () => {
        this.toast.success('Cập nhật sản phẩm thành công!');
        this.loadData();
        this.closeModal();
        this.isSaving.set(false);
      },
      error: (err) => {
        const msg = err?.error?.message || 'Lỗi cập nhật sản phẩm';
        this.toast.error(msg);
        this.isSaving.set(false);
      },
    });
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
