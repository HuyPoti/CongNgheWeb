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

  proceedToPayment() {
    if (!this.firstName || !this.lastName || !this.phone || !this.addressLine) {
      this.toastService.warning('Vui lòng điền đầy đủ thông tin giao hàng');
      return;
    }
    
    void this.router.navigate(['/cart/payment'], {
      state: {
        shippingAddress: {
          recipientName: `${this.firstName} ${this.lastName}`.trim(),
          phone: this.phone.trim(),
          addressLine: this.addressLine.trim(),
        }
      }
    });
  }
}
