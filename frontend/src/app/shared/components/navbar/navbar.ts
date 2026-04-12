import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ThemeService } from '../../../core/services/theme';
import { CommonModule } from '@angular/common';
import { SearchOverlay } from '../search-overlay/search-overlay';
import { LanguageService } from '../../../core/services/language.service';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { CartService } from '../../../core/services/cart.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, CommonModule, SearchOverlay, TranslatePipe],
  templateUrl: './navbar.html',
  styles: ``,
})
export class Navbar {
  themeService = inject(ThemeService);
  langService = inject(LanguageService);
  cartService = inject(CartService);
  isSearchOpen = signal(false);

  // Wrappers for template
  get currentTheme() { return this.themeService.theme(); }
  get isThemeForced() { return this.themeService.isForced(); }
  
  cartCount = this.cartService.totalItems;

  toggleSearch() {
    this.isSearchOpen.update(v => !v);
  }

  toggleTheme() {
    this.themeService.toggleTheme();
  }

  toggleLanguage() {
    this.langService.toggleLanguage();
  }
}
