import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormsModule,
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
} from '@angular/forms';
import { SupplierService } from '../../../core/services/supplier.service';
import { ToastService } from '../../../core/services/toast.service';
import { ConfirmService } from '../../../core/services/confirm.service';
import {
  Supplier,
  CreateSupplierDto,
  UpdateSupplierDto,
} from '../../../core/models/supplier.model';

@Component({
  selector: 'app-supplier-management',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './supplier-management.html',
})
export class SupplierManagement implements OnInit {
  private supplierService = inject(SupplierService);
  private fb = inject(FormBuilder);
  private toastService = inject(ToastService);
  private confirmService = inject(ConfirmService);

  suppliers = signal<Supplier[]>([]);
  isLoading = signal<boolean>(false);
  isSaving = signal<boolean>(false);

  showModal = signal<boolean>(false);
  isEditMode = signal<boolean>(false);
  currentSupplierId = signal<string | null>(null);

  searchQuery = signal<string>('');

  supplierForm: FormGroup;

  filteredSuppliers = computed(() => {
    const query = this.searchQuery().toLowerCase().trim();
    const list = this.suppliers();
    if (!query) return list;
    return list.filter(
      (s) =>
        s.name.toLowerCase().includes(query) ||
        (s.contactName && s.contactName.toLowerCase().includes(query)) ||
        (s.email && s.email.toLowerCase().includes(query)) ||
        (s.phone && s.phone.includes(query)) ||
        (s.taxCode && s.taxCode.includes(query)),
    );
  });

  constructor() {
    this.supplierForm = this.fb.group({
      name: ['', Validators.required],
      contactName: [''],
      phone: ['', [Validators.pattern(/^[0-9+ \-()]{7,20}$/)]],
      email: ['', [Validators.email]],
      address: [''],
      taxCode: [''],
      isActive: [true, Validators.required],
    });
  }

  ngOnInit() {
    this.loadSuppliers();
  }

  loadSuppliers() {
    this.isLoading.set(true);
    this.supplierService.getSuppliers().subscribe({
      next: (data) => {
        this.suppliers.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Lỗi khi tải danh sách NCC', err);
        this.toastService.error('Không thể tải danh sách nhà cung cấp');
        this.isLoading.set(false);
      },
    });
  }

  getSupplierCount() {
    return this.suppliers().length;
  }

  openCreateModal() {
    this.isEditMode.set(false);
    this.currentSupplierId.set(null);
    this.supplierForm.reset({ isActive: true });
    this.showModal.set(true);
  }

  openEditModal(supplier: Supplier) {
    this.isEditMode.set(true);
    this.currentSupplierId.set(supplier.supplierId);
    this.supplierForm.patchValue({
      name: supplier.name,
      contactName: supplier.contactName,
      phone: supplier.phone,
      email: supplier.email,
      address: supplier.address,
      taxCode: supplier.taxCode,
      isActive: supplier.isActive,
    });
    this.showModal.set(true);
  }

  closeModal() {
    this.showModal.set(false);
  }

  saveSupplier() {
    if (this.supplierForm.invalid) {
      this.toastService.warning('Vui lòng kiểm tra lại thông tin biểu mẫu');
      return;
    }

    this.isSaving.set(true);
    const formValue = this.supplierForm.value;

    if (this.isEditMode() && this.currentSupplierId()) {
      const dto: UpdateSupplierDto = formValue;
      this.supplierService.updateSupplier(this.currentSupplierId()!, dto).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.toastService.success('Cập nhật nhà cung cấp thành công');
          this.closeModal();
          this.loadSuppliers();
        },
        error: (err) => {
          console.error('Lỗi khi cập nhật NCC', err);
          this.toastService.error('Lỗi khi cập nhật nhà cung cấp');
          this.isSaving.set(false);
        },
      });
    } else {
      const dto: CreateSupplierDto = formValue;
      this.supplierService.createSupplier(dto).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.toastService.success('Thêm nhà cung cấp mới thành công');
          this.closeModal();
          this.loadSuppliers();
        },
        error: (err) => {
          console.error('Lỗi khi tạo NCC', err);
          this.toastService.error('Lỗi khi tạo nhà cung cấp mới');
          this.isSaving.set(false);
        },
      });
    }
  }

  async deleteSupplier(supplier: Supplier) {
    const isConfirmed = await this.confirmService.confirm(
      `Bạn có chắc chắn muốn xóa nhà cung cấp "${supplier.name}"?`,
      'Xóa nhà cung cấp',
      'danger',
    );

    if (isConfirmed) {
      this.supplierService.deleteSupplier(supplier.supplierId).subscribe({
        next: () => {
          this.toastService.success('Xóa nhà cung cấp thành công');
          this.loadSuppliers();
        },
        error: (err) => {
          console.error('Lỗi khi xóa NCC', err);
          this.toastService.error('Lỗi khi xóa nhà cung cấp');
        },
      });
    }
  }
}
