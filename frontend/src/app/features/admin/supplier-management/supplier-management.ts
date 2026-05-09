import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SupplierService } from '../../../core/services/supplier.service';
import { Supplier, CreateSupplierDto, UpdateSupplierDto } from '../../../core/models/supplier.model';

@Component({
  selector: 'app-supplier-management',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './supplier-management.html'
})
export class SupplierManagement implements OnInit {
  private supplierService = inject(SupplierService);
  private fb = inject(FormBuilder);

  suppliers = signal<Supplier[]>([]);
  isLoading = signal<boolean>(false);
  isSaving = signal<boolean>(false);
  
  showModal = signal<boolean>(false);
  isEditMode = signal<boolean>(false);
  currentSupplierId = signal<string | null>(null);

  supplierForm: FormGroup;

  constructor() {
    this.supplierForm = this.fb.group({
      name: ['', Validators.required],
      contactName: [''],
      phone: [''],
      email: ['', [Validators.email]],
      address: [''],
      taxCode: [''],
      status: [1, Validators.required]
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
        this.isLoading.set(false);
      }
    });
  }

  openCreateModal() {
    this.isEditMode.set(false);
    this.currentSupplierId.set(null);
    this.supplierForm.reset({ status: 1 });
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
      status: supplier.status
    });
    this.showModal.set(true);
  }

  closeModal() {
    this.showModal.set(false);
  }

  saveSupplier() {
    if (this.supplierForm.invalid) return;

    this.isSaving.set(true);
    const formValue = this.supplierForm.value;

    if (this.isEditMode() && this.currentSupplierId()) {
      const dto: UpdateSupplierDto = formValue;
      this.supplierService.updateSupplier(this.currentSupplierId()!, dto).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.closeModal();
          this.loadSuppliers();
        },
        error: (err) => {
          console.error('Lỗi khi cập nhật NCC', err);
          this.isSaving.set(false);
        }
      });
    } else {
      const dto: CreateSupplierDto = formValue;
      this.supplierService.createSupplier(dto).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.closeModal();
          this.loadSuppliers();
        },
        error: (err) => {
          console.error('Lỗi khi tạo NCC', err);
          this.isSaving.set(false);
        }
      });
    }
  }

  deleteSupplier(id: string) {
    if (confirm('Bạn có chắc chắn muốn xóa nhà cung cấp này?')) {
      this.supplierService.deleteSupplier(id).subscribe({
        next: () => {
          this.loadSuppliers();
        },
        error: (err) => {
          console.error('Lỗi khi xóa NCC', err);
        }
      });
    }
  }
}
