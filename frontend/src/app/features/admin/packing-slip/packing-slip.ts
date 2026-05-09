import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OrderDetailDto } from '../../../core/models/order.model';
import { ShipmentDto } from '../../../core/services/shipment.service';

@Component({
  selector: 'app-packing-slip',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="packing-slip-container text-black bg-white">
      <div class="header border-b-2 border-black pb-4 mb-4">
        <h1 class="text-3xl font-black uppercase text-center mb-2">Phiếu Giao Hàng</h1>
        <div class="flex justify-between items-end">
          <div>
            <p class="font-bold">Mã Đơn: {{ order.orderCode }}</p>
            <p>Ngày tạo: {{ shipment?.createdAt | date:'dd/MM/yyyy HH:mm' }}</p>
          </div>
          <div class="text-right">
            <p class="font-bold text-xl">{{ shipment?.carrier }}</p>
            @if (shipment?.trackingCode) {
              <p>Mã VĐ: {{ shipment?.trackingCode }}</p>
            }
          </div>
        </div>
      </div>

      <div class="grid grid-cols-2 gap-8 mb-8">
        <div class="border border-gray-300 p-4 rounded">
          <h3 class="font-bold uppercase mb-2 border-b border-gray-200 pb-1">Người nhận</h3>
          <p class="font-bold text-lg">{{ order.shippingAddress.recipientName }}</p>
          <p class="mt-1">{{ order.shippingAddress.phone }}</p>
          <p class="mt-1">{{ order.shippingAddress.addressLine }}</p>
          <p>{{ order.shippingAddress.ward }}, {{ order.shippingAddress.district }}, {{ order.shippingAddress.province }}</p>
        </div>
        
        <div class="border border-gray-300 p-4 rounded">
          <h3 class="font-bold uppercase mb-2 border-b border-gray-200 pb-1">Ghi chú vận chuyển</h3>
          <p class="italic text-gray-700 min-h-[4rem]">{{ order.notes || 'Không có ghi chú' }}</p>
          <div class="mt-4 flex items-center justify-between font-bold">
            <span>Thu hộ (COD):</span>
            <span class="text-xl">{{ (order.paymentMethod === 'cod' && order.paymentStatus === 'unpaid') ? ('$' + order.totalAmount) : 'Không thu tiền' }}</span>
          </div>
        </div>
      </div>

      <div class="items-list mb-8">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="border-b-2 border-black">
              <th class="py-2 w-16 text-center">STT</th>
              <th class="py-2">Tên sản phẩm</th>
              <th class="py-2 w-24 text-center">SL</th>
            </tr>
          </thead>
          <tbody>
            @for (item of order.items; track item.orderItemId; let i = $index) {
              <tr class="border-b border-gray-300">
                <td class="py-3 text-center">{{ i + 1 }}</td>
                <td class="py-3 font-bold">{{ item.productName }}</td>
                <td class="py-3 text-center text-lg font-black">{{ item.quantity }}</td>
              </tr>
            }
          </tbody>
        </table>
      </div>

      <div class="footer mt-12 flex justify-between px-8">
        <div class="text-center">
          <p class="font-bold">Người Đóng Gói</p>
          <p class="text-sm text-gray-500 mt-1">(Ký và ghi rõ họ tên)</p>
          <div class="mt-16 font-bold">{{ shipment?.packedByName || '' }}</div>
        </div>
        <div class="text-center">
          <p class="font-bold">Người Nhận Hàng</p>
          <p class="text-sm text-gray-500 mt-1">(Ký và ghi rõ họ tên)</p>
          <div class="mt-16"></div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .packing-slip-container {
      font-family: 'Times New Roman', Times, serif;
      padding: 2rem;
      max-width: 210mm;
      margin: 0 auto;
    }
  `]
})
export class PackingSlipComponent {
  @Input() order!: OrderDetailDto;
  @Input() shipment?: ShipmentDto | null;
}
