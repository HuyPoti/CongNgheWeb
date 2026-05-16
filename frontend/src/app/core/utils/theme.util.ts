import { Injectable, signal, effect, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root',
})
export class ThemeService {
  private platformId = inject(PLATFORM_ID);

  // Active storage key — switches per layout context
  private _storageKey = 'theme';

  // Signal to manage active theme: 'light' | 'dark'
  private _theme = signal<string>(this.getInitialTheme('theme'));

  // Track if user can toggle (always true in new design)
  private _isForced = signal<boolean>(false);

  readonly theme = this._theme.asReadonly();
  readonly isForced = this._isForced.asReadonly();

  constructor() {
    // Apply theme to DOM whenever signal changes
    effect(() => {
      if (isPlatformBrowser(this.platformId)) {
        this.applyThemeToRoot(this._theme());
      }
    });
  }

  // Returns stored theme for a key, defaults to 'light'
  private getInitialTheme(key: string): string {
    if (!isPlatformBrowser(this.platformId)) return 'light';
    return localStorage.getItem(key) ?? 'light';
  }

  // Toggle between light/dark — persists to current context key
  toggleTheme() {
    const nextTheme = this._theme() === 'dark' ? 'light' : 'dark';
    this._theme.set(nextTheme);
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(this._storageKey, nextTheme);
    }
  }

  /**
   * Switch to a named layout context.
   * Each context has its own localStorage key (theme_admin, theme_employee)
   * and defaults to 'light' on first visit.
   */
  setContext(context: 'admin' | 'employee' | null) {
    this._storageKey = context ? `theme_${context}` : 'theme';
    this._isForced.set(false);
    this._theme.set(this.getInitialTheme(this._storageKey));
  }

  /** @deprecated kept for backward compatibility — use setContext() */
  setForcedTheme(theme: 'light' | 'dark' | null) {
    if (theme) {
      this._isForced.set(true);
      this._theme.set(theme);
    } else {
      this._isForced.set(false);
      this._theme.set(this.getInitialTheme(this._storageKey));
    }
  }

  private applyThemeToRoot(theme: string) {
    if (!isPlatformBrowser(this.platformId)) return;
    const root = document.documentElement;
    if (theme === 'dark') {
      root.classList.add('dark');
    } else {
      root.classList.remove('dark');
    }
  }
}
