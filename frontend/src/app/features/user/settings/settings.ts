import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ThemeService } from '../../../core/services/theme';
import { LanguageService, Language } from '../../../core/services/language.service';
import { TranslatePipe } from '../../../core/pipes/translate.pipe';

@Component({
  selector: 'app-settings',
  imports: [CommonModule, TranslatePipe],
  templateUrl: './settings.html',
})
export class Settings {
  themeService = inject(ThemeService);
  langService = inject(LanguageService);

  activeTab = signal('account');

  tabs = [
    { id: 'account', label: 'settings.tab_account', icon: 'person_settings' },
    { id: 'security', label: 'settings.tab_security', icon: 'security' },
    { id: 'notifications', label: 'settings.tab_notifications', icon: 'notifications' },
    { id: 'preferences', label: 'settings.tab_preferences', icon: 'tune' },
  ];

  updateTab(tabId: string) {
    this.activeTab.set(tabId);
  }

  setTheme(theme: 'light' | 'dark') {
    if (this.themeService.theme() !== theme) {
      this.themeService.toggleTheme();
    }
  }

  setLanguage(lang: Language) {
    this.langService.setLanguage(lang);
  }
}
