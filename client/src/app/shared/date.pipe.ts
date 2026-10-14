import { Pipe, PipeTransform, inject } from '@angular/core';
import { I18nService } from '../core/i18n/i18n.service';

// "2023-01-01" -> "Oca 2023" / "Jan 2023"   |   monthYear: 'year' -> "2023"
// Intl.DateTimeFormat tarayıcının kendi tarih formatlayıcısı, ekstra kütüphane gerekmiyor
@Pipe({ name: 'monthYear', pure: false })
export class MonthYearPipe implements PipeTransform {
  private readonly i18n = inject(I18nService);
  private readonly formatters = {
    tr: new Intl.DateTimeFormat('tr-TR', { month: 'short', year: 'numeric' }),
    en: new Intl.DateTimeFormat('en-US', { month: 'short', year: 'numeric' })
  };

  transform(value: string | null | undefined, mode: 'month' | 'year' = 'month'): string {
    if (!value) return '';
    // CV'de olduğu gibi sadece yıl göstermek istenirse ay bilgisine hiç bakmıyoruz
    if (mode === 'year') return value.slice(0, 4);
    // NOT: "2023-01-01" string'ini new Date ile okuyunca UTC kabul ediliyor, saat dilimi yüzünden
    // bir önceki aya kayabiliyor. Yıl ve ayı elle ayırıp yerel tarih oluşturuyorum.
    const [year, month] = value.split('-').map(Number);
    return this.formatters[this.i18n.lang()].format(new Date(year, (month || 1) - 1, 1));
  }
}
