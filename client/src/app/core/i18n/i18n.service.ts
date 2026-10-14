import { DOCUMENT, Injectable, effect, inject, signal } from '@angular/core';
import { Lang, TranslationKey, translations } from './translations';
import { Localized } from '../models';

const STORAGE_KEY = 'pp-lang';

@Injectable({ providedIn: 'root' })
export class I18nService {
  private readonly document = inject(DOCUMENT);

  // NOT: Dil bir signal. Değişince onu okuyan her template otomatik yeniden çiziliyor,
  // DevLog'daki gibi BehaviorSubject + subscribe yazmaya gerek kalmıyor.
  readonly lang = signal<Lang>(this.initialLang());

  constructor() {
    effect(() => {
      const lang = this.lang();
      this.document.documentElement.lang = lang;
      try {
        localStorage.setItem(STORAGE_KEY, lang);
      } catch {
        // gizli sekme vb. durumlarda localStorage hata verebilir, önemli değil
      }
    });
  }

  toggle(): void {
    this.lang.update((l) => (l === 'tr' ? 'en' : 'tr'));
  }

  t(key: TranslationKey): string {
    return translations[this.lang()][key] ?? key;
  }

  // API'den gelen { tr, en } nesnesinden seçili dili döndürür, boşsa diğer dile düşer
  pick(value: Localized | null | undefined): string {
    if (!value) return '';
    const lang = this.lang();
    return (lang === 'tr' ? value.tr || value.en : value.en || value.tr) ?? '';
  }

  private initialLang(): Lang {
    try {
      const saved = localStorage.getItem(STORAGE_KEY);
      if (saved === 'tr' || saved === 'en') return saved;
    } catch {
      /* yok say */
    }
    return navigator.language?.toLowerCase().startsWith('tr') ? 'tr' : 'en';
  }
}
