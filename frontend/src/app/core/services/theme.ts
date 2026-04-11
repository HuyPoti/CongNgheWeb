import { Injectable, signal, effect, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root',
})
export class ThemeService {
  private platformId = inject(PLATFORM_ID);
  
  // Signal to manage active theme: 'light' | 'dark'
  private _theme = signal<string>(this.getInitialTheme());
  
  // Property to track if we're forcing a theme (e.g. forced Dark in Admin)
  private _isForced = signal<boolean>(false);
  
  readonly theme = this._theme.asReadonly();
  readonly isForced = this._isForced.asReadonly();

  constructor() {
    // Effect to apply theme whenever it changes
    effect(() => {
      if (isPlatformBrowser(this.platformId)) {
        const activeTheme = this._theme();
        this.applyThemeToRoot(activeTheme);
      }
    });
  }

  // Returns initial theme from localStorage or system preference
  private getInitialTheme(): string {
    if (!isPlatformBrowser(this.platformId)) return 'dark';
    
    const stored = localStorage.getItem('theme');
    if (stored) return stored;
    return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
  }

  // Manually toggle theme
  toggleTheme() {
    if (this._isForced()) return; // Don't allow toggle if forced
    const nextTheme = this._theme() === 'dark' ? 'light' : 'dark';
    this._theme.set(nextTheme);
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem('theme', nextTheme);
    }
  }

  // Helper to force a theme (used by layouts)
  setForcedTheme(theme: 'light' | 'dark' | null) {
    if (theme) {
      this._isForced.set(true);
      this._theme.set(theme);
    } else {
      this._isForced.set(false);
      // Revert to user preference
      this._theme.set(this.getInitialTheme());
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
