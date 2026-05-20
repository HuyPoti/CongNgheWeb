import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ConfirmService } from '../../../core/services/confirm.service';

@Component({
  selector: 'app-confirm',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (confirmService.isOpen()) {
      <div 
        class="fixed inset-0 bg-black/60 backdrop-blur-sm z-[99999] flex items-center justify-center p-4" 
        (click)="confirmService.decline()"
        (keydown.escape)="confirmService.decline()"
        role="button"
        tabindex="0"
      >
        <div 
          class="confirm-card w-full max-w-sm bg-white dark:bg-slate-900 border border-slate-200 dark:border-slate-800 p-5 rounded-2xl shadow-2xl flex flex-col gap-4 pointer-events-auto"
          (click)="$event.stopPropagation()"
          (keydown)="$event.stopPropagation()"
          role="dialog"
          aria-modal="true"
        >
          <div class="flex items-start gap-4">
            <!-- Icon Badge -->
            <div 
              class="shrink-0 size-10 rounded-xl flex items-center justify-center mt-0.5"
              [ngClass]="{
                'bg-yellow-100 dark:bg-yellow-950 text-yellow-600 dark:text-yellow-400': confirmService.type() === 'warning',
                'bg-red-100 dark:bg-red-950 text-red-600 dark:text-red-400': confirmService.type() === 'danger',
                'bg-blue-100 dark:bg-blue-950 text-blue-600 dark:text-blue-400': confirmService.type() === 'info'
              }"
            >
              <span class="material-symbols-outlined text-xl leading-none">
                {{ confirmService.type() === 'warning' ? 'warning' :
                   confirmService.type() === 'danger' ? 'delete_forever' : 'help' }}
              </span>
            </div>

            <!-- Content -->
            <div class="flex-1 min-w-0">
              <h3 class="text-sm font-black uppercase tracking-wider text-slate-800 dark:text-white mb-1">
                {{ confirmService.title() }}
              </h3>
              <p class="text-sm text-slate-600 dark:text-slate-300 leading-snug break-words">
                {{ confirmService.message() }}
              </p>
            </div>
          </div>

          <!-- Actions -->
          <div class="flex items-center justify-end gap-2.5 mt-2">
            <button 
              (click)="confirmService.decline()"
              class="px-4 py-2 text-xs font-bold text-slate-500 hover:text-slate-800 dark:text-slate-400 dark:hover:text-white hover:bg-slate-100 dark:hover:bg-white/10 rounded-xl transition-all"
            >
              Hủy
            </button>
            <button 
              (click)="confirmService.approve()"
              class="px-4 py-2 text-xs font-bold rounded-xl shadow-lg transition-all"
              [ngClass]="{
                'bg-yellow-500 hover:bg-yellow-600 text-slate-900 shadow-yellow-500/20': confirmService.type() === 'warning',
                'bg-red-500 hover:bg-red-600 text-white shadow-red-500/20': confirmService.type() === 'danger',
                'bg-blue-500 hover:bg-blue-600 text-white shadow-blue-500/20': confirmService.type() === 'info'
              }"
            >
              Xác nhận
            </button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    @keyframes zoom-in-bounce {
      from { transform: scale(0.95); opacity: 0; }
      to   { transform: scale(1);   opacity: 1; }
    }
    .confirm-card {
      animation: zoom-in-bounce 0.25s cubic-bezier(0.34, 1.56, 0.64, 1) forwards;
    }
  `]
})
export class ConfirmComponent {
  confirmService = inject(ConfirmService);
}
