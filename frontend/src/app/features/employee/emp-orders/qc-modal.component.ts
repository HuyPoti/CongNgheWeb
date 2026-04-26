import { Component, Output, EventEmitter, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-qc-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './qc-modal.component.html'
})
export class QcModalComponent {
  @Input() orderId = '';
  @Output() closed = new EventEmitter<void>();
  @Output() submitted = new EventEmitter<{ notes: string }>();
  
  checklist = [
    { id: 1, label: 'Kiểm tra bao bì nguyên vẹn, không rách/móp', checked: false },
    { id: 2, label: 'Xác nhận đúng Model & SKU', checked: false },
    { id: 3, label: 'Kiểm tra tem niêm phong (Seal) của nhà sản xuất', checked: false },
    { id: 4, label: 'Kiểm tra đầy đủ phụ kiện, cáp nguồn, hướng dẫn', checked: false },
    { id: 5, label: 'Dán tem bảo hành GearVN đúng vị trí', checked: false }
  ];

  qcNotes = '';

  get isAllChecked() {
    return this.checklist.every(item => item.checked);
  }

  submitQC() {
    if (!this.isAllChecked) return;
    this.submitted.emit({ notes: this.qcNotes });
    alert('Đã xác nhận QC và đóng gói thành công!');
    this.closed.emit();
  }

  onClose() {
    this.closed.emit();
  }
}
