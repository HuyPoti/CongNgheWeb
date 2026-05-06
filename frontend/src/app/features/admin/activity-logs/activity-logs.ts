import { Component, inject, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivityLogService, ActivityLogDto, PagedResult } from '../../../core/services/activity-log.service';

@Component({
  selector: 'app-activity-logs',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="p-8">
      <div class="flex items-center gap-4 mb-10">
        <div class="w-14 h-14 bg-primary/10 text-primary rounded-2xl flex items-center justify-center shadow-[0_0_15px_rgba(0,229,255,0.2)]">
          <span class="material-symbols-outlined text-3xl">history</span>
        </div>
        <div>
          <h1 class="text-3xl font-black text-slate-100 tracking-tight uppercase italic font-neon">Nhật ký hoạt động</h1>
          <p class="text-slate-400 font-medium">Theo dõi tất cả thay đổi trong hệ thống</p>
        </div>
      </div>

      <!-- Filters -->
      <div class="grid grid-cols-1 md:grid-cols-4 gap-4 mb-6">
        <input type="text" placeholder="Tìm theo action..." [(ngModel)]="filterAction" class="px-4 py-2 rounded-lg bg-surface-dark border border-primary/20 text-slate-100" />
        <input type="text" placeholder="Entity type..." [(ngModel)]="filterEntityType" class="px-4 py-2 rounded-lg bg-surface-dark border border-primary/20 text-slate-100" />
        <input type="date" [(ngModel)]="filterFromDate" class="px-4 py-2 rounded-lg bg-surface-dark border border-primary/20 text-slate-100" />
        <button (click)="loadLogs()" class="px-4 py-2 rounded-lg bg-primary text-background-dark font-bold hover:bg-primary/90">Lọc</button>
      </div>

      <!-- Table -->
      <div class="bg-surface-dark border border-primary/10 rounded-3xl overflow-hidden">
        <table class="w-full text-left">
          <thead>
            <tr class="bg-primary/5 border-b border-primary/10">
              <th class="px-6 py-5 text-xs font-black text-primary uppercase tracking-widest">Thời gian</th>
              <th class="px-6 py-5 text-xs font-black text-primary uppercase tracking-widest">User</th>
              <th class="px-6 py-5 text-xs font-black text-primary uppercase tracking-widest">Hành động</th>
              <th class="px-6 py-5 text-xs font-black text-primary uppercase tracking-widest">Entity</th>
              <th class="px-6 py-5 text-xs font-black text-primary uppercase tracking-widest">IP Address</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-primary/5">
            @if (loading()) {
              <tr><td colspan="5" class="text-center py-10 text-slate-400">Đang tải...</td></tr>
            } @else if (logs().length === 0) {
              <tr><td colspan="5" class="text-center py-10 text-slate-400">Không có dữ liệu</td></tr>
            } @else {
              @for (log of logs(); track log.logId) {
                <tr class="hover:bg-primary/5 transition-colors">
                  <td class="px-6 py-5 text-sm text-slate-400">{{ log.createdAt | date:'dd/MM/yyyy HH:mm' }}</td>
                  <td class="px-6 py-5 text-sm text-slate-100">{{ log.userName || 'System' }}</td>
                  <td class="px-6 py-5 text-sm">
                    <span class="px-3 py-1 bg-primary/10 text-primary rounded-full text-xs font-bold">{{ log.action }}</span>
                  </td>
                  <td class="px-6 py-5 text-sm text-slate-400">{{ log.entityType || '-' }}</td>
                  <td class="px-6 py-5 text-sm text-slate-400">{{ log.ipAddress || '-' }}</td>
                </tr>
              }
            }
          </tbody>
        </table>
      </div>

      <!-- Pagination -->
      @if (totalCount() > pageSize()) {
        <div class="flex justify-center gap-2 mt-6">
          <button (click)="prevPage()" [disabled]="page() === 1" class="px-4 py-2 rounded-lg bg-primary/10 text-primary disabled:opacity-50">Trang trước</button>
          <span class="px-4 py-2 text-slate-400">{{ page() }} / {{ Math.ceil(totalCount() / pageSize()) }}</span>
          <button (click)="nextPage()" [disabled]="page() >= Math.ceil(totalCount() / pageSize())" class="px-4 py-2 rounded-lg bg-primary/10 text-primary disabled:opacity-50">Trang sau</button>
        </div>
      }
    </div>
  `
})
export class ActivityLogsComponent {
  private activityLogService = inject(ActivityLogService);

  logs = signal<ActivityLogDto[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  page = signal(1);
  pageSize = signal(20);
  totalCount = signal(0);
  
  filterAction = signal('');
  filterEntityType = signal('');
  filterFromDate = signal('');
  filterToDate = signal('');

  Math = Math;

  constructor() {
    effect(() => {
      this.loadLogs();
    });
  }

  ngOnInit() {
    this.loadLogs();
  }

  loadLogs() {
    this.loading.set(true);
    this.error.set(null);
    
    this.activityLogService.getAll({
      page: this.page(),
      pageSize: this.pageSize(),
      entityType: this.filterEntityType() || undefined,
      fromDate: this.filterFromDate() || undefined,
      toDate: this.filterToDate() || undefined
    }).subscribe({
      next: (res: PagedResult<ActivityLogDto>) => {
        this.logs.set(res.items);
        this.totalCount.set(res.totalCount);
        this.loading.set(false);
      },
      error: (err) => {
        this.error.set('Lỗi tải dữ liệu: ' + (err?.message || 'Unknown error'));
        this.loading.set(false);
      }
    });
  }

  prevPage() {
    if (this.page() > 1) {
      this.page.set(this.page() - 1);
    }
  }

  nextPage() {
    const maxPage = Math.ceil(this.totalCount() / this.pageSize());
    if (this.page() < maxPage) {
      this.page.set(this.page() + 1);
    }
  }
}`

