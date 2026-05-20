import { Component, inject, signal, effect, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CouponService, CouponDto, PagedResult } from '../../../core/services/coupon.service';
import { ToastService } from '../../../core/services/toast.service';
import { ConfirmService } from '../../../core/services/confirm.service';
import { CouponFormComponent } from './coupon-form';

@Component({
  selector: 'app-coupon-list',
  standalone: true,
  imports: [CommonModule, FormsModule, CouponFormComponent],
  templateUrl: './coupon-list.html'
})
export class CouponListComponent implements OnInit {
  private couponService = inject(CouponService);
  private toast = inject(ToastService);
  private confirmService = inject(ConfirmService);

  coupons = signal<CouponDto[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  page = signal(1);
  pageSize = signal(10);
  totalCount = signal(0);
  keyword = signal('');

  isFormOpen = signal(false);
  isEditMode = signal(false);
  selectedCoupon = signal<CouponDto | null>(null);

  Math = Math; // expose Math for template

  constructor() {
    effect(() => {
      this.loadCoupons();
    });
  }

  ngOnInit() {
    this.loadCoupons();
  }

  loadCoupons() {
    this.loading.set(true);
    this.error.set(null);
    
    this.couponService.getAll({
      page: this.page(),
      pageSize: this.pageSize(),
      keyword: this.keyword() || undefined
    }).subscribe({
      next: (res: PagedResult<CouponDto>) => {
        this.coupons.set(res.items);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Lỗi tải dữ liệu: ' + (err?.message || 'Unknown error'));
        this.loading.set(false);
      }
    });
  }

  openCreateForm() {
    this.isEditMode.set(false);
    this.selectedCoupon.set(null);
    this.isFormOpen.set(true);
  }

  openEditForm(coupon: CouponDto) {
    this.isEditMode.set(true);
    this.selectedCoupon.set(coupon);
    this.isFormOpen.set(true);
  }

  closeForm() {
    this.isFormOpen.set(false);
    this.selectedCoupon.set(null);
  }

  onFormSave() {
    this.loadCoupons();
  }

  async deactivateCoupon(id: string) {
    const isConfirmed = await this.confirmService.confirm(
      'Bạn có chắc chắn muốn vô hiệu hóa mã giảm giá này? Hành động này không thể hoàn tác.',
      'Vô hiệu hóa mã',
      'danger'
    );
    if (isConfirmed) {
      this.couponService.deactivate(id).subscribe({
        next: () => {
          this.toast.success('Vô hiệu hóa mã giảm giá thành công!');
          this.loadCoupons();
        },
        error: (err) => {
          this.toast.error('Lỗi: ' + (err?.message || 'Cannot deactivate coupon'));
        }
      });
    }
  }
}
