import { Component, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-coupon-input',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="bg-white p-6 rounded-3xl border border-gray-100 shadow-sm">
      <h3 class="text-sm font-black text-gray-900 uppercase tracking-widest mb-4 flex items-center gap-2">
        <i class="fas fa-ticket-alt text-primary"></i>
        Mã giảm giá
      </h3>
      <div class="flex gap-2">
        <input type="text" [(ngModel)]="couponCode" 
               placeholder="Nhập mã ưu đãi..."
               class="flex-1 bg-gray-50 border border-gray-100 p-3 rounded-xl focus:outline-none focus:ring-2 focus:ring-primary/20 text-sm font-bold uppercase tracking-widest">
        <button (click)="apply()" 
                [disabled]="!couponCode"
                class="bg-gray-900 hover:bg-primary text-white font-bold px-6 py-3 rounded-xl transition-all disabled:opacity-50">
          Áp dụng
        </button>
      </div>
      @if (applied) {
        <div class="mt-3 flex items-center justify-between bg-green-50 p-3 rounded-xl border border-green-100">
          <div class="flex items-center gap-2">
            <i class="fas fa-check-circle text-green-500 text-xs"></i>
            <span class="text-xs font-bold text-green-700">Đã áp dụng mã: {{ couponCode }}</span>
          </div>
          <button (click)="remove()" class="text-red-400 hover:text-red-600 transition-colors">
            <i class="fas fa-times text-xs"></i>
          </button>
        </div>
      }
    </div>
  `,
})
export class CouponInputComponent {
  couponCode = '';
  applied = false;

  @Output() couponApplied = new EventEmitter<string>();

  apply() {
    this.applied = true;
    this.couponApplied.emit(this.couponCode);
    alert(`Đã áp dụng mã ${this.couponCode} thành công!`);
  }

  remove() {
    this.applied = false;
    this.couponCode = '';
  }
}
