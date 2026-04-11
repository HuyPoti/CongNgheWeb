import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';

@Component({
  selector: 'app-profile',
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './profile.html',
  styleUrl: './profile.css',
})
export class Profile {
  // Mock user data
  user = {
    name: 'Minh Nguyen',
    email: 'minh.nguyen@email.com',
    phone: '+84 912 345 678',
    memberSince: '2024-01-15',
    tier: 'Gold',
    points: 4250,
    totalOrders: 23,
    totalSpent: 12499,
  };
}
