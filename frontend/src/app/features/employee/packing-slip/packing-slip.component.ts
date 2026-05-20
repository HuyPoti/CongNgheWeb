import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { OrderService } from '../../../core/services/order.service';
import { ShipmentService, ShipmentDto } from '../../../core/services/shipment.service';
import { OrderDetailDto } from '../../../core/models/order.model';

@Component({
  selector: 'app-packing-slip',
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: './packing-slip.component.html',
  styleUrls: ['./packing-slip.component.css']
})
export class PackingSlipComponent implements OnInit {
  private router   = inject(Router);
  private route    = inject(ActivatedRoute);
  private orderSvc = inject(OrderService);
  private shipSvc  = inject(ShipmentService);

  order    = signal<OrderDetailDto | null>(null);
  shipment = signal<ShipmentDto | null>(null);
  isLoading = signal(true);
  error     = signal<string | null>(null);

  readonly today = new Date();

  ngOnInit() {
    const orderId = this.route.snapshot.queryParamMap.get('id');
    if (!orderId) {
      this.error.set('Không tìm thấy mã đơn hàng trong URL.');
      this.isLoading.set(false);
      return;
    }

    this.orderSvc.getById(orderId).subscribe({
      next: (order) => {
        this.order.set(order);

        // Load shipment in parallel
        this.shipSvc.getByOrderId(orderId).subscribe({
          next: (s) => this.shipment.set(s),
          error: () => this.shipment.set(null)
        });

        this.isLoading.set(false);
      },
      error: () => {
        this.error.set('Không thể tải thông tin đơn hàng.');
        this.isLoading.set(false);
      }
    });
  }

  print() {
    window.print();
  }

  goBack() {
    this.router.navigate(['/employee/orders']);
  }
}
