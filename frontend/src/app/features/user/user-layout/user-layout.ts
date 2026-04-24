import { Component, inject } from '@angular/core';
import { AuthService } from '../../../core/services/auth.service';
import { RouterLink, RouterOutlet, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';

@Component({
  selector: 'app-user-layout',
  imports: [RouterLink, RouterOutlet, CommonModule, TranslatePipe],
  template: `
    <div
      class="min-h-screen bg-background-light dark:bg-background-dark transition-all duration-500"
    >
      <div class="max-w-7xl mx-auto px-4 md:px-8 py-8 md:py-12">
        <div class="grid grid-cols-1 lg:grid-cols-12 gap-8 lg:gap-12 items-start">
          <!-- Sidebar -->
          <aside class="lg:col-span-3 lg:sticky lg:top-24">
            <div
              class="bg-white dark:bg-surface-dark rounded-[2.5rem] border border-slate-100 dark:border-white/5 shadow-2xl shadow-slate-200/40 dark:shadow-none overflow-hidden animate-in fade-in slide-in-from-left-4 duration-700"
            >
              <!-- User Profile Header -->
              @if (authService.currentUser$ | async; as user) {
                <div
                  class="p-10 text-center border-b border-slate-50 dark:border-white/5 bg-slate-50/50 dark:bg-white/2"
                >
                  <div class="relative w-28 h-28 mx-auto mb-6 group">
                    <div
                      class="absolute inset-0 bg-primary/20 rounded-[2rem] blur-xl opacity-0 group-hover:opacity-100 transition-opacity"
                    ></div>

                    @if (user.avatarUrl) {
                      <img
                        [src]="user.avatarUrl"
                        class="relative w-full h-full rounded-[2rem] object-cover shadow-xl rotate-3 group-hover:rotate-0 transition-all duration-500"
                        [alt]="user.fullName"
                      />
                    } @else {
                      <div
                        class="relative w-full h-full rounded-[2rem] bg-gradient-to-br from-primary to-accent-cyan flex items-center justify-center text-white text-4xl font-black shadow-xl shadow-primary/20 rotate-3 group-hover:rotate-0 transition-all duration-500"
                      >
                        {{ getInitials(user.fullName) }}
                      </div>
                    }

                    <div
                      class="absolute -bottom-1 -right-1 size-8 rounded-2xl bg-white dark:bg-surface-dark border-4 border-slate-50 dark:border-white/5 flex items-center justify-center shadow-lg"
                    >
                      <span class="material-symbols-outlined text-primary text-sm font-black"
                        >verified</span
                      >
                    </div>
                  </div>
                  <h3 class="font-bold text-2xl text-slate-900 dark:text-white tracking-tight">
                    {{ user.fullName }}
                  </h3>
                </div>
              }

              <!-- Navigation Menu -->
              <nav class="p-6 space-y-3">
                @for (item of sidebarItems; track item.id) {
                  <a
                    [routerLink]="item.route"
                    class="flex items-center gap-4 px-6 py-4 rounded-2xl text-[11px] font-black uppercase tracking-[0.2em] transition-all duration-300 cursor-pointer group whitespace-nowrap"
                    [class]="
                      isActive(item.route)
                        ? 'bg-primary text-white shadow-xl shadow-primary/30 scale-[1.02]'
                        : 'text-slate-500 dark:text-slate-400 hover:bg-slate-50 dark:hover:bg-white/5 hover:text-primary hover:translate-x-1'
                    "
                  >
                    <span
                      class="material-symbols-outlined text-xl transition-transform group-hover:rotate-6"
                      >{{ item.icon }}</span
                    >
                    {{ item.label | translate }}
                  </a>
                }

                <div class="pt-6 mt-6 border-t border-slate-50 dark:border-white/5">
                  <button
                    (click)="logout()"
                    class="w-full flex items-center gap-4 px-6 py-4 rounded-2xl text-[11px] font-black uppercase tracking-[0.2em] text-red-500 hover:bg-red-50 dark:hover:bg-red-500/10 transition-all cursor-pointer group"
                  >
                    <span
                      class="material-symbols-outlined text-xl transition-transform group-hover:-translate-x-1"
                      >logout</span
                    >
                    {{ 'user.sidebar_logout' | translate }}
                  </button>
                </div>
              </nav>
            </div>
          </aside>

          <!-- Main Viewport -->
          <main class="lg:col-span-9 animate-in fade-in slide-in-from-right-4 duration-700">
            <div
              class="bg-white/30 dark:bg-white/2 backdrop-blur-sm rounded-[3rem] border border-slate-100 dark:border-white/5 p-4 lg:p-10 min-h-[800px]"
            >
              <router-outlet></router-outlet>
            </div>
          </main>
        </div>
      </div>
    </div>
  `,
  styles: ``,
})
export class UserLayout {
  private router = inject(Router);
  authService = inject(AuthService);

  sidebarItems = [
    { id: 'profile', label: 'user.sidebar_overview', icon: 'person', route: '/user/profile' },
    { id: 'orders', label: 'user.sidebar_orders', icon: 'receipt_long', route: '/user/orders' },
    {
      id: 'tracking',
      label: 'user.sidebar_tracking',
      icon: 'radar',
      route: '/user/order-tracking',
    },
    { id: 'settings', label: 'user.sidebar_settings', icon: 'settings', route: '/user/settings' },
  ];

  isActive(route: string): boolean {
    return this.router.url === route;
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/auth/login']);
  }

  getInitials(name: string): string {
    return name
      ? name
          .split(' ')
          .map((n) => n[0])
          .join('')
          .toUpperCase()
          .substring(0, 2)
      : 'U';
  }
}
