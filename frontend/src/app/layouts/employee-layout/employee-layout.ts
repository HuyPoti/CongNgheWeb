import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ThemeService } from '../../core/utils/theme.util';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-employee-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './employee-layout.html',
  styleUrl: './employee-layout.css',
})
export class EmployeeLayout implements OnInit, OnDestroy {
  readonly themeService = inject(ThemeService);
  private authService = inject(AuthService);
  private router = inject(Router);

  ngOnInit() {
    // Switch to employee context — persists theme_employee key, defaults to 'light'
    this.themeService.setContext('employee');
  }

  get currentRole(): string {
    return this.authService.currentUserValue?.role?.toLowerCase() ?? '';
  }

  get isWarehouse(): boolean {
    return this.currentRole === 'warehouse';
  }

  get isStaff(): boolean {
    return this.currentRole === 'staff';
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/portal']);
  }

  ngOnDestroy() {
    // Revert to user context when leaving employee panel
    this.themeService.setContext(null);
  }
}
