import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ThemeService } from '../../core/utils/theme.util';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, CommonModule],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.css',
})
export class AdminLayout implements OnInit, OnDestroy {
  readonly themeService = inject(ThemeService);
  private authService = inject(AuthService);
  private router = inject(Router);

  ngOnInit() {
    // Switch to admin context — persists theme_admin key, defaults to 'light'
    this.themeService.setContext('admin');
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['/portal']);
  }

  ngOnDestroy() {
    // Revert to user context when leaving admin
    this.themeService.setContext(null);
  }
}
