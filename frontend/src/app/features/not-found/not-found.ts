import { Component, inject, OnInit } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './not-found.html',
})
export class NotFoundComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);

  homeUrl = '/';

  ngOnInit() {
    const user = this.authService.currentUserValue;
    if (user) {
      const role = user.role.toLowerCase();
      if (role === 'admin') {
        this.homeUrl = '/admin/dashboard';
      } else if (role === 'staff') {
        this.homeUrl = '/employee/orders';
      } else if (role === 'warehouse') {
        this.homeUrl = '/employee/warehouse-orders';
      } else {
        this.homeUrl = '/';
      }
    }
  }
}
