import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';

@Component({
  selector: 'app-orders',
  imports: [CommonModule, RouterLink, TranslatePipe],
  templateUrl: './orders.html',
  styleUrl: './orders.css',
})
export class Orders {}
