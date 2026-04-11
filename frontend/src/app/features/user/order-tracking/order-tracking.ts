import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';

@Component({
  selector: 'app-order-tracking',
  imports: [CommonModule, TranslatePipe],
  templateUrl: './order-tracking.html',
  styles: ``,
})
export class OrderTracking {}
