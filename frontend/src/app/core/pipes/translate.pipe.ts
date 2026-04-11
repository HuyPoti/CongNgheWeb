import { Pipe, PipeTransform, inject } from '@angular/core';
import { LanguageService } from '../services/language.service';
import { TRANSLATIONS } from '../constants/translations';

@Pipe({
  name: 'translate',
  standalone: true,
  pure: false // Important: needed to react to signal changes
})
export class TranslatePipe implements PipeTransform {
  private langService = inject(LanguageService);

  transform(key: string): string {
    const lang = this.langService.lang();
    const dictionary = (TRANSLATIONS as any)[lang] || TRANSLATIONS.vi;
    
    return dictionary[key] || key;
  }
}
