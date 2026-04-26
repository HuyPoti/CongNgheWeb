import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-flash-sale-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './flash-sale-list.html'
})
export class FlashSaleListComponent {
  mockFlashSales = [
    { id: 1, title: 'Đại tiệc công nghệ tháng 4', startTime: new Date('2026-04-25T00:00:00'), endTime: new Date('2026-04-26T00:00:00'), status: 'Đang diễn ra', itemCount: 12, active: true },
    { id: 2, title: 'Flash Sale Cuối Tuần', startTime: new Date('2026-05-01T12:00:00'), endTime: new Date('2026-05-01T14:00:00'), status: 'Sắp diễn ra', itemCount: 5, active: true },
    { id: 3, title: 'Xả kho RTX 30-series', startTime: new Date('2026-04-20T00:00:00'), endTime: new Date('2026-04-21T00:00:00'), status: 'Đã kết thúc', itemCount: 20, active: false }
  ];

  deleteFlashSale(id: number) {
    if(confirm('Bạn có chắc muốn xóa chương trình này?')) {
      this.mockFlashSales = this.mockFlashSales.filter(x => x.id !== id);
    }
  }
}
