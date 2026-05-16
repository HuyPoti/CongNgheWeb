import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="fixed top-5 right-5 z-[9999] flex flex-col items-end gap-3 pointer-events-none w-full max-w-sm">
      @for (toast of toastService.toasts(); track toast.id) {
        <div
          class="toast-item w-full flex items-start gap-3 px-4 py-3.5 rounded-2xl shadow-xl border pointer-events-auto"
          [ngClass]="{
            'bg-emerald-500 dark:bg-slate-800 border-green-200 dark:border-green-700': toast.type === 'success',
            'bg-red-500 dark:bg-slate-800 border-red-200 dark:border-red-700':     toast.type === 'error',
            'bg-blue-500 dark:bg-slate-800 border-blue-200 dark:border-blue-700':   toast.type === 'info',
            'bg-yellow-500 dark:bg-slate-800 border-yellow-200 dark:border-yellow-700': toast.type === 'warning'
          }"
        >
          <!-- Icon badge -->
          <div
            class="shrink-0 size-8 rounded-xl flex items-center justify-center mt-0.5"
            [ngClass]="{
              'bg-green-100 dark:bg-green-900 text-green-600 dark:text-green-400':   toast.type === 'success',
              'bg-red-100 dark:bg-red-900 text-red-600 dark:text-red-400':           toast.type === 'error',
              'bg-blue-100 dark:bg-blue-900 text-blue-600 dark:text-blue-400':       toast.type === 'info',
              'bg-yellow-100 dark:bg-yellow-900 text-yellow-600 dark:text-yellow-400': toast.type === 'warning'
            }"
          >
            <span class="material-symbols-outlined text-base leading-none">
              {{ toast.type === 'success' ? 'check_circle' :
                 toast.type === 'error'   ? 'error' :
                 toast.type === 'info'    ? 'info'  : 'warning' }}
            </span>
          </div>

          <!-- Text -->
          <div class="flex-1 min-w-0">
            <p
              class="text-xs font-black uppercase tracking-widest mb-0.5"
              [ngClass]="{
                'text-green-700 dark:text-green-900':   toast.type === 'success',
                'text-red-700 dark:text-red-900':       toast.type === 'error',
                'text-blue-700 dark:text-blue-900':     toast.type === 'info',
                'text-yellow-700 dark:text-yellow-900': toast.type === 'warning'
              }"
            >
              {{ toast.type === 'success' ? 'Thành công' :
                 toast.type === 'error'   ? 'Lỗi' :
                 toast.type === 'info'    ? 'Thông báo' : 'Cảnh báo' }}
            </p>
            <p class="text-sm text-slate-700 dark:text-white/90 leading-snug break-words">{{ toast.message }}</p>
          </div>

          <!-- Close button -->
          <button
            (click)="toastService.remove(toast.id)"
            class="shrink-0 size-7 flex items-center justify-center rounded-lg text-slate-400 hover:text-slate-600 dark:hover:text-slate-200 hover:bg-slate-100 dark:hover:bg-white/10 transition-all mt-0.5"
          >
            <span class="material-symbols-outlined text-base">close</span>
          </button>
        </div>
      }
    </div>
  `,
  styles: [`
    @keyframes slide-in-right {
      from { transform: translateX(110%); opacity: 0; }
      to   { transform: translateX(0);   opacity: 1; }
    }
    .toast-item {
      animation: slide-in-right 0.35s cubic-bezier(0.34, 1.56, 0.64, 1) forwards;
    }
  `]
})
export class ToastComponent {
  toastService = inject(ToastService);
}

