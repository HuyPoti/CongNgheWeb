import { Injectable, signal, effect, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

export type Language = 'en' | 'vi';

@Injectable({
  providedIn: 'root',
})
export class LanguageService {
  private platformId = inject(PLATFORM_ID);
  private _lang = signal<Language>(this.getInitialLanguage());
  
  readonly lang = this._lang.asReadonly();

  constructor() {
    effect(() => {
      if (isPlatformBrowser(this.platformId)) {
        const currentLang = this._lang();
        localStorage.setItem('language', currentLang);
        document.documentElement.lang = currentLang;
      }
    });
  }

  private getInitialLanguage(): Language {
    if (!isPlatformBrowser(this.platformId)) return 'vi';
    
    const stored = localStorage.getItem('language') as Language;
    if (stored === 'en' || stored === 'vi') return stored;
    
    // Default to Vietnamese if system language is Vietnamese
    return window.navigator.language.startsWith('vi') ? 'vi' : 'en';
  }

  setLanguage(lang: Language) {
    this._lang.set(lang);
  }

  toggleLanguage() {
    this._lang.update(l => l === 'en' ? 'vi' : 'en');
  }

  // Simple translate helper (can be expanded)
  translate(key: string, translations: Record<Language, string>): string {
    return translations[this._lang()];
  }
}
