import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-verified-badge',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (isVerified) {
      <div class="flex items-center gap-1.5 px-2.5 py-1 bg-emerald-50 text-emerald-600 rounded-full border border-emerald-100/50 shadow-sm">
        <span class="material-symbols-outlined text-[14px] font-bold">verified</span>
        <span class="text-[10px] font-extrabold uppercase tracking-wider">Đã mua hàng</span>
      </div>
    }
  `
})
export class VerifiedBadgeComponent {
  @Input() isVerified = false;
}
