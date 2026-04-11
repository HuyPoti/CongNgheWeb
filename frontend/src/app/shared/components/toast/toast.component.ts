import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="fixed top-5 left-1/2 -translate-x-1/2 z-[100] flex flex-col items-center gap-3 pointer-events-none w-full max-w-sm">
      @for (toast of toastService.toasts(); track toast.id) {
        <div
          class="flex items-center gap-3 px-6 py-4 rounded-2xl shadow-2xl backdrop-blur-md border animate-slide-in pointer-events-auto"
          [ngClass]="{
            'bg-green-500/20 border-green-500/50 text-green-400': toast.type === 'success',
            'bg-red-500/20 border-red-500/50 text-red-500': toast.type === 'error',
            'bg-blue-500/20 border-blue-500/50 text-blue-400': toast.type === 'info',
            'bg-yellow-500/20 border-yellow-500/50 text-yellow-400': toast.type === 'warning'
          }"
        >
          <span class="material-symbols-outlined text-lg">
            {{ toast.type === 'success' ? 'check_circle' : 
               toast.type === 'error' ? 'error' : 
               toast.type === 'info' ? 'info' : 'warning' }}
          </span>
          <span class="text-sm font-semibold">{{ toast.message }}</span>
          <button (click)="toastService.remove(toast.id)" class="ml-4 hover:opacity-70 transition-opacity">
            <span class="material-symbols-outlined text-base">close</span>
          </button>
        </div>
      }
    </div>
  `,
  styles: [`
    @keyframes slide-in {
      from { transform: translateY(-20px); opacity: 0; }
      to { transform: translateY(0); opacity: 1; }
    }
    .animate-slide-in {
      animation: slide-in 0.3s cubic-bezier(0.34, 1.56, 0.64, 1) forwards;
    }
  `]
})
export class ToastComponent {
  toastService = inject(ToastService);
}
