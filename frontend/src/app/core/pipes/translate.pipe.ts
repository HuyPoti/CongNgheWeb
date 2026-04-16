import { Pipe, PipeTransform, inject } from '@angular/core';
import { LanguageService } from '../services/language.service';
import { TRANSLATIONS } from '../constants/translations';

@Pipe({
  name: 'translate',
  standalone: true,
  pure: false,
})
export class TranslatePipe implements PipeTransform {
  private langService = inject(LanguageService);

  transform(key: string, params?: Record<string, any>): string {
    const lang = this.langService.lang();
    const dictionary = (TRANSLATIONS as any)[lang] || TRANSLATIONS.vi;

    let text = dictionary[key] || key;

    if (params) {
      for (const k of Object.keys(params)) {
        text = text.replace(new RegExp(`\\[\\[${k}\\]\\]`, 'g'), String(params[k]));
      }
    }

    return text;
  }
}
