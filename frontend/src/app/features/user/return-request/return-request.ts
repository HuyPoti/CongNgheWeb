import { Component, inject, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ReturnRequestService, CreateReturnRequest } from '../../../core/services/return-request.service';
import { OrderService } from '../../../core/services/order.service';
import { ToastService } from '../../../core/services/toast.service';
import { CloudinaryService } from '../../../core/services/cloudinary.service';
import { OrderDetailDto, OrderItemDto } from '../../../core/models/order.model';

interface SelectedReturnItem extends OrderItemDto {
  selected: boolean;
  returnQuantity: number;
  reasonDetail: string;
}

@Component({
  selector: 'app-return-request',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './return-request.html'
})
export class ReturnRequestComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private location = inject(Location);
  private returnService = inject(ReturnRequestService);
  private orderService = inject(OrderService);
  private toast = inject(ToastService);
  private cloudinaryService = inject(CloudinaryService);
  private cdr = inject(ChangeDetectorRef);

  orderId = '';
  order?: OrderDetailDto;
  isLoading = true;
  isSubmitting = false;
  isUploading = false;

  reasons = [
    { id: 'Sản phẩm lỗi kỹ thuật', label: 'Sản phẩm lỗi kỹ thuật' },
    { id: 'Giao sai sản phẩm', label: 'Giao sai sản phẩm' },
    { id: 'Sản phẩm không đúng mô tả', label: 'Sản phẩm không đúng mô tả' },
    { id: 'Đổi ý / Không còn nhu cầu', label: 'Không còn nhu cầu / Đổi ý' },
    { id: 'Lý do khác', label: 'Lý do khác' }
  ];
  
  selectedReason = '';
  description = '';
  uploadedImages: string[] = [];
  
  items: SelectedReturnItem[] = [];

  ngOnInit(): void {
    // Ưu tiên lấy từ param (:id), fallback sang queryParam (?orderId=)
    this.orderId = this.route.snapshot.paramMap.get('id') || 
                   this.route.snapshot.queryParamMap.get('orderId') || '';
                   
    if (!this.orderId) {
      this.toast.error('Không tìm thấy mã đơn hàng');
      this.router.navigate(['/user/orders']);
      return;
    }
    this.loadOrder();
  }

  loadOrder(): void {
    this.isLoading = true;
    this.cdr.detectChanges(); // Fix NG0100

    this.orderService.getById(this.orderId).subscribe({
      next: (order) => {
        this.order = order;
        this.items = order.items.map(i => ({
          ...i,
          selected: true,
          returnQuantity: i.quantity,
          reasonDetail: ''
        }));
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.toast.error('Không thể tải thông tin đơn hàng');
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  goBack(): void {
    this.location.back();
  }

  submitRequest(): void {
    if (!this.selectedReason) {
      this.toast.warning('Vui lòng chọn lý do đổi trả!');
      return;
    }

    const selectedItems = this.items.filter(i => i.selected);
    if (selectedItems.length === 0) {
      this.toast.warning('Vui lòng chọn ít nhất một sản phẩm!');
      return;
    }

    this.isSubmitting = true;
    this.cdr.detectChanges();

    const dto: CreateReturnRequest = {
      orderId: this.orderId,
      reason: this.selectedReason,
      description: this.description,
      items: selectedItems.map(i => ({
        orderItemId: i.orderItemId,
        quantity: i.returnQuantity,
        reasonDetail: i.reasonDetail
      })),
      imageUrls: this.uploadedImages
    };

    this.returnService.create(dto).subscribe({
      next: () => {
        this.toast.success('Gửi yêu cầu đổi trả thành công!');
        this.router.navigate(['/user/orders']);
      },
      error: (err) => {
        this.toast.error(err.error?.message || 'Có lỗi xảy ra khi gửi yêu cầu');
        this.isSubmitting = false;
        this.cdr.detectChanges();
      }
    });
  }

  onImageUpload(event: Event): void {
    const element = event.target as HTMLInputElement;
    const file = element.files?.[0];
    if (!file) return;

    // Validate
    const error = this.cloudinaryService.validateImageFile(file);
    if (error) {
      this.toast.error(error);
      return;
    }

    this.isUploading = true;
    this.cdr.detectChanges();

    this.cloudinaryService.uploadImage('returns', file).subscribe({
      next: (res) => {
        this.uploadedImages.push(res.imageUrl);
        this.isUploading = false;
        this.toast.success('Đã tải ảnh lên thành công');
        this.cdr.detectChanges();
      },
      error: () => {
        this.toast.error('Lỗi khi tải ảnh lên');
        this.isUploading = false;
        this.cdr.detectChanges();
      }
    });
  }

  removeImage(index: number): void {
    this.uploadedImages.splice(index, 1);
  }
}
