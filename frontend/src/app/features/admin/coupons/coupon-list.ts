import { Component, inject, signal, effect, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CouponService, CouponDto, PagedResult } from '../../../core/services/coupon.service';
import { CouponFormComponent } from './coupon-form';

@Component({
  selector: 'app-coupon-list',
  standalone: true,
  imports: [CommonModule, FormsModule, CouponFormComponent],
  templateUrl: './coupon-list.html'
})
export class CouponListComponent implements OnInit {
  private couponService = inject(CouponService);

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

  deactivateCoupon(id: string, coupon: CouponDto) {
    if(confirm('Bạn có chắc muốn vô hiệu hóa mã này?')) {
      this.couponService.deactivate(id).subscribe({
        next: () => {
          this.loadCoupons();
        },
        error: (err) => {
          alert('Lỗi: ' + (err?.message || 'Cannot deactivate coupon'));
        }
      });
    }
  }
}
