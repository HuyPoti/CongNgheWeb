import { Component, OnInit, PLATFORM_ID, inject, signal } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ConfirmService } from '../../../core/services/confirm.service';
import { FlashSaleDto, FlashSaleService, PagedResult } from '../../../core/services/flash-sale.service';
import { ToastService } from '../../../core/services/toast.service';
import { FlashSaleFormComponent } from './flash-sale-form';
import { FlashSaleItemsComponent } from './flash-sale-items';

@Component({
  selector: 'app-flash-sale-list',
  standalone: true,
  imports: [CommonModule, FormsModule, FlashSaleFormComponent, FlashSaleItemsComponent],
  templateUrl: './flash-sale-list.html'
})
export class FlashSaleListComponent implements OnInit {
  private readonly flashSaleService = inject(FlashSaleService);
  private readonly toast = inject(ToastService);
  private readonly confirmService = inject(ConfirmService);
  private readonly platformId = inject(PLATFORM_ID);

  flashSales = signal<FlashSaleDto[]>([]);
  loading = signal(false);
  page = signal(1);
  pageSize = signal(10);
  totalCount = signal(0);

  isFormOpen = signal(false);
  isEditMode = signal(false);
  selectedFlashSale = signal<FlashSaleDto | null>(null);
  isItemsOpen = signal(false);

  ngOnInit() {
    if (!isPlatformBrowser(this.platformId)) return;
    this.loadFlashSales();
  }

  loadFlashSales() {
    if (!isPlatformBrowser(this.platformId)) return;

    this.loading.set(true);

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
        this.loading.set(false);
        this.toast.error(err?.error?.message || 'Khong the tai danh sach flash sale');
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
    this.closeForm();
    this.loadFlashSales();
  }

  getStatus(fs: FlashSaleDto): string {
    if (!fs.isActive) return 'Da tat';

    const now = new Date();
    const start = new Date(fs.startTime);
    const end = new Date(fs.endTime);

    if (now < start) return 'Sap dien ra';
    if (now > end) return 'Da ket thuc';
    return 'Dang dien ra';
  }

  async deleteFlashSale(id: string) {
    const isConfirmed = await this.confirmService.confirm(
      'Ban co chac muon xoa mem chuong trinh flash sale nay?',
      'Xoa mem flash sale',
      'danger'
    );
    if (!isConfirmed) return;

    this.loading.set(true);

    this.flashSaleService.delete(id).subscribe({
      next: () => {
        this.toast.success('Da xoa mem flash sale thanh cong');
        this.loadFlashSales();
      },
      error: (err) => {
        this.loading.set(false);
        this.toast.error(err?.error?.message || 'Khong the xoa mem flash sale');
      }
    });
  }
}
