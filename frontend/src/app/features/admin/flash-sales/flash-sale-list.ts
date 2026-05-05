import { Component, inject, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { FlashSaleService, FlashSaleDto, PagedResult } from '../../../core/services/flash-sale.service';
import { FlashSaleFormComponent } from './flash-sale-form';
import { FlashSaleItemsComponent } from './flash-sale-items';

@Component({
  selector: 'app-flash-sale-list',
  standalone: true,
  imports: [CommonModule, FormsModule, FlashSaleFormComponent, FlashSaleItemsComponent],
  templateUrl: './flash-sale-list.html'
})
export class FlashSaleListComponent {
  private flashSaleService = inject(FlashSaleService);

  flashSales = signal<FlashSaleDto[]>([]);
  loading = signal(false);
  error = signal<string | null>(null);
  page = signal(1);
  pageSize = signal(10);
  totalCount = signal(0);

  isFormOpen = signal(false);
  isEditMode = signal(false);
  selectedFlashSale = signal<FlashSaleDto | null>(null);
  isItemsOpen = signal(false);

  constructor() {
    effect(() => {
      this.loadFlashSales();
    });
  }

  ngOnInit() {
    this.loadFlashSales();
  }

  loadFlashSales() {
    this.loading.set(true);
    this.error.set(null);
    
    this.flashSaleService.getAll({
      page: this.page(),
      pageSize: this.pageSize()
    }).subscribe({
      next: (res: PagedResult<FlashSaleDto>) => {
        this.flashSales.set(res.items);
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
    this.selectedFlashSale.set(null);
    this.isFormOpen.set(true);
  }

  openEditForm(flashSale: FlashSaleDto) {
    this.isEditMode.set(true);
    this.selectedFlashSale.set(flashSale);
    this.isFormOpen.set(true);
  }

  openItemsModal(flashSale: FlashSaleDto) {
    this.selectedFlashSale.set(flashSale);
    this.isItemsOpen.set(true);
  }

  closeForm() {
    this.isFormOpen.set(false);
    this.selectedFlashSale.set(null);
  }

  closeItemsModal() {
    this.isItemsOpen.set(false);
    this.selectedFlashSale.set(null);
  }

  onFormSave() {
    this.loadFlashSales();
  }

  getStatus(fs: FlashSaleDto): string {
    const now = new Date();
    const start = new Date(fs.startTime);
    const end = new Date(fs.endTime);
    
    if (now < start) return 'Sắp diễn ra';
    if (now > end) return 'Đã kết thúc';
    return 'Đang diễn ra';
  }

  deleteFlashSale(id: string) {
    if(confirm('Bạn có chắc muốn xóa chương trình này?')) {
      // Backend doesn't have delete endpoint yet - can implement PUT to mark inactive or add DELETE
      alert('Chức năng xóa chưa hỗ trợ - vui lòng liên hệ admin');
    }
  }
}

