import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { ThemeService } from '../../core/services/theme';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './admin-layout.html',
  styleUrl: './admin-layout.css',
})
export class AdminLayout implements OnInit, OnDestroy {
  private themeService = inject(ThemeService);

  ngOnInit() {
    // Admin section is exclusively Dark Mode
    this.themeService.setForcedTheme('dark');
  }

  ngOnDestroy() {
    // Revert to user preference when leaving admin
    this.themeService.setForcedTheme(null);
  }
}
