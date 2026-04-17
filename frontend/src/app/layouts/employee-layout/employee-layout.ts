import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { ThemeService } from '../../core/services/theme';

@Component({
  selector: 'app-employee-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './employee-layout.html',
  styleUrl: './employee-layout.css',
})
export class EmployeeLayout implements OnInit, OnDestroy {
  private themeService = inject(ThemeService);

  ngOnInit() {
    this.themeService.setForcedTheme('dark');
  }

  ngOnDestroy() {
    this.themeService.setForcedTheme(null);
  }
}
