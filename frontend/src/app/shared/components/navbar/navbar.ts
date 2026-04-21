import { Component, DestroyRef, inject, signal, computed } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { ThemeService } from '../../../core/services/theme';
import { CommonModule } from '@angular/common';
import { SearchOverlay } from '../search-overlay/search-overlay';
import { LanguageService } from '../../../core/services/language.service';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';
import { CartService } from '../../../core/services/cart.service';
import { CategoryService } from '../../../core/services/category.service';
import { Category } from '../../../core/models/category.model';
import { MegaMenu } from '../mega-menu/mega-menu';
import { catchError, of } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink, CommonModule, SearchOverlay, TranslatePipe, MegaMenu],
  templateUrl: './navbar.html',
  styles: ``,
})
export class Navbar {
  private destroyRef = inject(DestroyRef);
  themeService = inject(ThemeService);
  langService = inject(LanguageService);
  cartService = inject(CartService);
  categoryService = inject(CategoryService);
  authService = inject(AuthService);
  isSearchOpen = signal(false);
  isMegaMenuOpen = signal(false);
  categories = signal<Category[]>([]);

  // Wrappers for template
  get currentTheme() {
    return this.themeService.theme();
  }
  get isThemeForced() {
    return this.themeService.isForced();
  }

  cartCount = this.cartService.totalItems;
  currentUser = toSignal(this.authService.currentUser$);
  isLoggedIn = computed(() => !!this.currentUser());

  constructor() {
    this.loadCategories();
  }

  toggleSearch() {
    this.isSearchOpen.update((v) => !v);
  }

  toggleTheme() {
    this.themeService.toggleTheme();
  }

  toggleLanguage() {
    this.langService.toggleLanguage();
  }

  openMegaMenu() {
    this.isMegaMenuOpen.set(true);
  }

  closeMegaMenu() {
    this.isMegaMenuOpen.set(false);
  }

  private loadCategories() {
    this.categoryService
      .getAll()
      .pipe(
        catchError(() => of([] as Category[])),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((data) => {
        this.categories.set(data.filter((c) => c.isActive));
      });
  }
}
