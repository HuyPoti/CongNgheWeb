import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-return-request',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './return-request.html'
})
export class ReturnRequestComponent {
  orderId = 'ORD-250426-X89';
  reasons = [
    { id: 'defective', label: 'Sản phẩm lỗi kỹ thuật' },
    { id: 'wrong_item', label: 'Giao sai sản phẩm' },
    { id: 'not_as_described', label: 'Sản phẩm không đúng mô tả' },
    { id: 'changed_mind', label: 'Không còn nhu cầu / Đổi ý' },
    { id: 'other', label: 'Lý do khác' }
  ];
  
  selectedReason = '';
  description = '';
  
  mockItems = [
    { id: 'd01', name: 'ASUS ROG Strix RTX 4090 OC', price: 55900000, quantity: 1, selected: false, image: 'https://dlcdnwebimgs.asus.com/gain/392DE691-8AF2-4FD0-8914-996489370894' }
  ];

  uploadedImages: string[] = [];

  submitRequest() {
    if (!this.selectedReason) {
      alert('Vui lòng chọn lý do đổi trả!');
      return;
    }
    console.log('Sending return request...', {
      reason: this.selectedReason,
      description: this.description,
      items: this.mockItems.filter(i => i.selected)
    });
    alert('Yêu cầu của bạn đã được gửi thành công. Nhân viên sẽ liên hệ lại sớm nhất!');
  }

  onImageUpload(event: any) {
    // Mock upload
    this.uploadedImages.push('https://placehold.co/100x100?text=Loi+San+Pham');
  }

  removeImage(index: number) {
    this.uploadedImages.splice(index, 1);
  }
}
