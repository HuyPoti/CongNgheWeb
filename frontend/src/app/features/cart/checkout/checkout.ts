import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CartService } from '../../../core/services/cart.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './checkout.html',
  styleUrl: './checkout.css',
})
export class Checkout {
  private router = inject(Router);
  private toastService = inject(ToastService);
  cartService = inject(CartService);

  firstName = '';
  lastName = '';
  phone = '';
  addressLine = '';
  province = '';
  ward = '';

  get fullAddress(): string {
    const parts = [
      this.addressLine.trim(),
      this.ward.trim(),
      this.province.trim(),
    ].filter(Boolean);
    return parts.join(', ');
  }

  proceedToPayment() {
    if (
      !this.firstName.trim() ||
      !this.lastName.trim() ||
      !this.phone.trim() ||
      !this.addressLine.trim() ||
      !this.province.trim() ||
      !this.ward.trim()
    ) {
      this.toastService.warning('Vui lòng điền đầy đủ thông tin giao hàng');
      return;
    }

    void this.router.navigate(['/cart/payment'], {
      state: {
        shippingAddress: {
          recipientName: `${this.firstName} ${this.lastName}`.trim(),
          phone: this.phone.trim(),
          addressLine: this.fullAddress,
          province: this.province.trim(),
          ward: this.ward.trim(),
        }
      }
    });
  }
}
